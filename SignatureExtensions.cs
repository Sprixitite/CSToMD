using System;
using System.Reflection;
using System.Text;
using System.Collections.Generic;

namespace CSToMD {

    public static class SignatureExtensions
    {
        /// <summary>
        /// Return the method signature as a string.
        /// </summary>
        ///
        /// <param name="property">
        /// The property to act on.
        /// </param>
        ///
        /// <returns>
        /// Method signature.
        /// </returns>

        public static string GetSignature(this PropertyInfo property, bool include_type)
        {
            var getter = property.GetGetMethod(true);
            var setter = property.GetSetMethod(true);

            var sigBuilder = new StringBuilder();
            var primaryDef = LeastRestrictiveVisibility(getter,setter);

               
            BuildReturnSignature(sigBuilder, primaryDef, include_type);
            sigBuilder.Append(" { ");
            if (getter!=null) {
                if (primaryDef != getter) {
                    sigBuilder.Append(Visibility(getter)+" ");
                }
                sigBuilder.Append("get; ");
            }
            if (setter!=null) {
                if (primaryDef != setter) {
                    sigBuilder.Append(Visibility(setter)+" ");
                }
                sigBuilder.Append("set; ");
            }
            sigBuilder.Append("}");
            return sigBuilder.ToString();

        }

        public static string GetSignature(this FieldInfo field, bool include_type) {
            StringBuilder sb = new StringBuilder("");
            if (field.IsPublic) sb.Append("public ");
            else if (field.IsPrivate) sb.Append("private ");
            else if (field.IsFamily) sb.Append("protected ");
            else if (field.IsAssembly) sb.Append("internal ");
            else if (field.IsFamilyOrAssembly) sb.Append("protected internal ");
            else if (field.IsFamilyAndAssembly) sb.Append("private protected");
            if (field.IsStatic && !field.IsLiteral) sb.Append("static ");
            else if (field.IsLiteral && !field.IsInitOnly) sb.Append("const ");
            else if (field.IsInitOnly) sb.Append("readonly ");
            if (include_type) { sb.Append(TypeName(field.FieldType)); sb.Append(" "); }
            sb.Append(field.Name);
            return sb.ToString();
        }

        public static string GetSignature(this ConstructorInfo method) {
            StringBuilder sb = new StringBuilder("");
            if (method.IsPublic) sb.Append("public ");
            else if (method.IsPrivate) sb.Append("private ");
            else if (method.IsFamily) sb.Append("protected ");
            else if (method.IsAssembly) sb.Append("internal ");
            else if (method.IsFamilyAndAssembly) sb.Append("private protected ");
            else if (method.IsFamilyOrAssembly) sb.Append("protected internal ");
            if (method.IsStatic) sb.Append("static ");
            sb.Append(TypeName(method.DeclaringType));
            BuildParamsSignature(sb, method);
            return sb.ToString();
        }

        public static string GetSignature(this EventInfo the_event, bool include_type) {
            StringBuilder sb = new StringBuilder("");
            MethodInfo adder = the_event.GetAddMethod(true);
            if (adder.IsPublic) sb.Append("public ");
            else if (adder.IsPrivate) sb.Append("private ");
            else if (adder.IsFamily) sb.Append("protected ");
            else if (adder.IsAssembly) sb.Append("internal ");
            else if (adder.IsFamilyAndAssembly) sb.Append("private protected ");
            else if (adder.IsFamilyOrAssembly) sb.Append("protected internal ");
            if (adder.IsStatic) sb.Append("static ");
            sb.Append("event ");
            if (include_type) { sb.Append(TypeName(the_event.EventHandlerType)); sb.Append(" "); }
            sb.Append(the_event.Name);
            return sb.ToString();
        }

        // public static string GetSignature(this Type t) {
        //     string end = TypeName(t);
        //     string start = ( t.isfa )
        // }

        /// <summary>
        /// Return the method signature as a string.
        /// </summary>
        ///
        /// <param name="method">
        /// The Method.
        /// </param>
        /// <param name="callable">
        /// Return as an callable string(public void a(string b) would return a(b))
        /// </param>
        ///
        /// <returns>
        /// Method signature.
        /// </returns>

        private static void BuildParamsSignature(StringBuilder sigBuilder, MethodBase method) {
            sigBuilder.Append("(");
            var firstParam = true;
            var secondParam = false;

            var parameters = method.GetParameters();

            foreach (var param in parameters)
            {
                if (firstParam)
                {
                    firstParam = false;
                    if (method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false))
                    {
                        sigBuilder.Append("this ");
                    }
                }
                else if (secondParam == true)
                    secondParam = false;
                else
                    sigBuilder.Append(", ");
                if (param.IsOut)
                    sigBuilder.Append("out ");
                else if (param.ParameterType.IsByRef)
                    sigBuilder.Append("ref ");

                if (IsParamArray(param))
                {
                    sigBuilder.Append("params ");
                }

                sigBuilder.Append(TypeName(param.ParameterType));
                sigBuilder.Append(' ');

                sigBuilder.Append(param.Name);

                if (param.IsOptional)
                {
                    sigBuilder.Append(" = " + 
                        ( param.DefaultValue ?? "null")
                    );
                }
            }
            sigBuilder.Append(")");
        }

        public static string GetSignature(this MethodInfo method, bool include_type)
        {
                
            var sigBuilder = new StringBuilder();

            BuildReturnSignature(sigBuilder,method, include_type);

            BuildParamsSignature(sigBuilder, method);

            // generic constraints
           

            foreach (var arg in method.GetGenericArguments())
            {
                List<string> constraints = new List<string>();
                foreach (var constraint in arg.GetGenericParameterConstraints())
                {
                    constraints.Add(TypeName(constraint));
                }

                var attrs = arg.GenericParameterAttributes;

                if (attrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint)) {
                    constraints.Add("class");
                }
                if (attrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint)) {
                    constraints.Add("struct");
                }
                if (attrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
                {
                    constraints.Add("new()");
                }
                if (constraints.Count > 0)
                {
                    sigBuilder.Append(" where " + TypeName(arg) + ": " + String.Join(", ", constraints));
                }
            }
     
            return sigBuilder.ToString();
        }

        public static Type GetReturnType(this MethodBase method) {
            try {
                return ((MethodInfo)method).ReturnType;
            } catch {
                return ((ConstructorInfo)method).DeclaringType;
            }
        }

        /// <summary>
        /// Get full type name with full namespace names.
        /// </summary>
        ///
        /// <param name="type">
        /// Type. May be generic or nullable.
        /// </param>
        ///
        /// <returns>
        /// Full type name, fully qualified namespaces.
        /// </returns>

        public static string TypeName(Type type)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
            {
                return TypeName(nullableType) + "?";
            }


            if (!type.IsGenericType)
            {
                if (type.IsArray)
                {
                    return TypeName(type.GetElementType()) + "[]";
                }

                //if (type.Si
                switch (type.Name)
                {
                    case "String": return "string";
                    case "Int16": return "short";
                    case "UInt16": return "ushort";          
                    case "Int32": return "int";
                    case "UInt32": return "uint";
                    case "Int64": return "long";
                    case "UInt64": return "ulong";
                    case "Decimal": return "decimal";
                    case "Double": return "double";
                    case "Object": return "object";
                    case "Void": return "void";
                    case "Boolean": return "bool";
                    case "Single": return "float";
                    case "Char": return "char";

                    default:
                    {
                        // return string.IsNullOrWhiteSpace(type.FullName) ? type.Name : type.FullName;
                        return type.Name;
                    }
                }
            }

            var sb = new StringBuilder(type.Name.Substring(0,
                type.Name.IndexOf('`'))
            );

            sb.Append('<');
            var first = true;
            foreach (var t in type.GetGenericArguments())
            {
                if (!first)
                    sb.Append(',');
                sb.Append(TypeName(t));
                first = false;
            }
            sb.Append('>');
            return sb.ToString();
        }

        private static void BuildReturnSignature(StringBuilder sigBuilder, MethodInfo method, bool include_type)
        {
            var firstParam = true;
            sigBuilder.Append(Visibility(method)+" ");

            if (method.IsStatic)
                sigBuilder.Append("static ");
            if (include_type) sigBuilder.Append(TypeName(method.ReturnType) + " ");
            sigBuilder.Append(method.Name);

            // Add method generics
            if (method.IsGenericMethod)
            {
                sigBuilder.Append("<");
                foreach (var g in method.GetGenericArguments())
                {
                    if (firstParam)
                        firstParam = false;
                    else
                        sigBuilder.Append(", ");
                    sigBuilder.Append(TypeName(g));
                }
                sigBuilder.Append(">");
            }

        }

        public static string GetSignature(this Type t) {
            StringBuilder sb = new StringBuilder("");
            if (t.IsPublic || t.IsNestedPublic) sb.Append("public ");
            if (t.IsNestedFamily) sb.Append("protected ");
            if (t.IsNestedAssembly || t.IsNotPublic) sb.Append("internal ");
            if (t.IsNestedPrivate) sb.Append("private ");
            if (t.IsNestedFamANDAssem) sb.Append("private protected ");
            if (t.IsNestedFamORAssem) sb.Append("protected internal ");
            if (t.IsSealed && t.IsAbstract) sb.Append("static ");
            else {
                if (t.IsSealed) sb.Append("sealed ");
                if (t.IsAbstract) sb.Append("abstract ");
            }
            sb.Append(t.GetArchetype().ToLower());
            sb.Append(SignatureExtensions.TypeName(t));
            if (t.IsClass) sb.Append((t.BaseType == typeof(Object) ? "" : " : " + t.BaseType.Name));
            return sb.ToString();
        }

        public static string GetArchetype(this Type t) {
            if (t.IsInterface) return "Interface ";
            if (t.IsEnum) return "Enum ";
            if (t.IsClass) return "Class ";
            if (t.IsValueType) return "Struct ";
            throw new NotImplementedException("Your class is not real.");
        }

        private static string Visibility(MethodInfo method) {
            if (method.IsPublic)
                return "public";
            else if (method.IsPrivate)
                return "private";
            else if (method.IsAssembly)
                return "internal";
            else if (method.IsFamily)
                return "protected";
            else if (method.IsFamilyAndAssembly)
                return "private protected";
            else if (method.IsFamilyOrAssembly)
                return "protected internal";
            else {
                throw new Exception("I wasn't able to parse the visibility of this method.");
            }
        }

        private static MethodInfo LeastRestrictiveVisibility(MethodInfo member1, MethodInfo member2)
        {
            if (member1!=null && member2==null) {
                return member1;
            } else if (member2!=null && member1==null) {
                return member2;
            }

            int vis1 = VisibilityValue(member1);
            int vis2 = VisibilityValue(member2);
            if (vis1<vis2) {
                return member1;
            } else {
                return member2;
            }
        }

        private static int VisibilityValue(MethodInfo method)
        {
            if (method.IsPublic)
                return 1;
            else if (method.IsFamilyOrAssembly)
                return 2;
            else if (method.IsFamily)
                return 3;
            else if (method.IsAssembly)
                return 4;
            else if (method.IsFamilyAndAssembly)
                return 5;
            else if (method.IsPrivate)
                return 6;
            else
            {
                throw new Exception("I wasn't able to parse the visibility of this method.");
            }
        }

        private static bool IsParamArray(ParameterInfo info)
        {
            return info.GetCustomAttribute(typeof(ParamArrayAttribute), true) != null;
        }
    }
}