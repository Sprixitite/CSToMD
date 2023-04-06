using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CSToMD {

    internal sealed class DocClass {

        public DocClass(Type _t) {

            parent = null;
            children = new List<DocClass>();
            members = new List<DocToken>();
            t = _t;
            foreach (MemberInfo mi in t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)) {
                if (Attribute.GetCustomAttribute(mi, typeof(CompilerGeneratedAttribute)) != null) continue;
                if (mi.GetType().IsSubclassOf(typeof(Type))) { children.Add(new DocClass((Type)mi, this)); continue; };
                DocToken a = DocToken.from(mi, t);
                members.Add( a );
            }

        }

        private DocClass(Type _t, DocClass p) : this(_t) {
            parent = p;
        }

        private List<DocToken> members;
        private List<DocClass> children;
        private DocClass parent;
        private Type t;

        public override string ToString() {

            StringBuilder sb = new StringBuilder("");
            sb.Append("| ");
            if (this.parent != null) sb.Append(SignatureExtensions.TypeName(this.parent.t));
            else sb.Append("N/A");
            sb.Append(" | ");
            sb.Append(t.GetSignature());
            sb.Append(" | ");
            sb.Append("N/A");
            sb.Append(" | ");
            sb.Append(t.GetArchetype().Trim());
            sb.Append(" | ");
            sb.Append("[ INSERT DESCRIPTION HERE ]");
            sb.Append(" | ");
            sb.Append("N/A |");

            foreach (DocToken token in members) {
                sb.Append("\n");
                sb.Append(token.ToString());
            }

            foreach (DocClass child in children) {
                sb.Append("\n" + (CSToMD.DEBUG ? "\n" : ""));
                sb.Append(child.ToString());
            }

            return sb.ToString();

        }

    }

}