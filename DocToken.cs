using System;
using System.Reflection;
using System.Text;

namespace CSToMD {

    internal abstract class DocToken {
        
        interface ISampleInterface
        {
            void SampleMethod();
        }

        enum SampleEnum {
            SAMPLE_VAL_1,
            SAMPLE_VAL_2
        }

        struct MyStruct {
            int a;
            int b;
        }

        protected DocToken(MemberInfo mi, Type o) {
            token_type = mi.MemberType;
            owner = o;
        }

        public static DocToken from(MemberInfo mi, Type o) {
            if ( mi.GetType().IsSubclassOf(typeof(FieldInfo)) ) return new DocFieldToken((FieldInfo)mi, o);
            if ( mi.GetType().IsSubclassOf(typeof(PropertyInfo)) ) return new DocPropertyToken((PropertyInfo)mi, o);
            if ( mi.GetType().IsSubclassOf(typeof(MethodBase)) ) return new DocMethodToken((MethodBase)mi, o);
            if ( mi.GetType().IsSubclassOf(typeof(EventInfo)) ) return new DocEventToken((EventInfo)mi, o);

            // Hopefully will never encounter this
            System.Console.WriteLine(mi.GetType().FullName);
            throw new NotImplementedException("There's another info type??");
        }

        public virtual string returns() => "N/A";
        public virtual string name() => "N/A";
        public virtual string description() => "N/A";
        public virtual string pseudocode() => "N/A";

        private Type owner;

        public MemberTypes token_type { get; private set; }

        public override string ToString() {
            StringBuilder sb = new StringBuilder("| ");
            sb.Append(SignatureExtensions.TypeName(owner));
            sb.Append(" | ");
            sb.Append(name());
            sb.Append(" | ");
            sb.Append(returns());
            sb.Append(" | ");
            sb.Append(token_type.ToString());
            sb.Append(" | ");
            sb.Append(description());
            sb.Append(" | ");
            sb.Append(pseudocode());
            sb.Append(" |");
            return sb.ToString();
        }

    }

    internal class DocFieldToken : DocToken {

        public DocFieldToken(FieldInfo _fi, Type o) : base(_fi, o) { fi = _fi; }

        private FieldInfo fi;

        public override string name() => fi.GetSignature(false);
        public override string returns() => SignatureExtensions.TypeName(fi.FieldType);

    }

    internal class DocPropertyToken : DocToken {

        public DocPropertyToken(PropertyInfo _pi, Type o) : base(_pi, o) { pi = _pi; }

        private PropertyInfo pi;

        public override string name() => pi.GetSignature(false);
        public override string returns() => SignatureExtensions.TypeName(pi.PropertyType);

    }

    internal class DocMethodToken : DocToken {

        public DocMethodToken(MethodBase _mi, Type o) : base(_mi, o) { mi = _mi; }

        private MethodBase mi;

        public override string name() {
            try {
                return ((MethodInfo)mi).GetSignature(false);
            } catch {
                return ((ConstructorInfo)mi).GetSignature();
            }
        }
        public override string pseudocode() => "[ INSERT PSEUDOCODE HERE ]";
        public override string returns() => SignatureExtensions.TypeName(mi.GetReturnType());
        public override string description() => "[ INSERT DESCRIPTION HERE ]";

    }

    internal class DocEventToken : DocToken {

        public DocEventToken(EventInfo _ei, Type o) : base(_ei, o) { ei = _ei; }

        private EventInfo ei;

        public override string name() => ei.GetSignature(false);
        public override string returns() { try { return SignatureExtensions.TypeName(ei.EventHandlerType); } catch { return "[ INSERT EVENT DELEGATE TYPE HERE ]"; } }
        public override string description() => "[ INSERT DESCRIPTION HERE ]";

    }

}