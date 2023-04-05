using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SprixDocGen {

    public static class Program {
        public static void Main(string[] args) => SprixDocGen.gen();
    }

    public static class SprixDocGen {

        internal const bool DEBUG = false;

        public static void gen() {

            FileStream markdown_file = File.Open("./output.md", FileMode.Create);
            StreamWriter markdown_out = new StreamWriter(markdown_file);
            markdown_out.WriteLine("| Parent Object | Signature | Datatype | Member Type | Description | Pseudocode |");
            markdown_out.WriteLine("|:-:|:--|:-:|:-:|:--|:--|");
            Assembly calling_assembly = Assembly.GetCallingAssembly();
            foreach ( Type t in calling_assembly.GetTypes() ) {
                if (Attribute.GetCustomAttribute(t, typeof(CompilerGeneratedAttribute)) != null) continue;
                if (t.IsNested) continue;
                string nextline = new DocClass(t).ToString();
                markdown_out.WriteLine(nextline);
                if (DEBUG) Console.WriteLine(nextline + "\n");
            }
            markdown_out.Close();
            markdown_file.Close();

        }

    }
                                                                                                                                                                                                            
    public class A {
        public int a;
        protected string b;
        private protected bool c;
        static internal A d;
        internal Dictionary<string, B<int, string>> my_list_of_b {
            get;
            private protected set;
        }
        private B<int, string> my_method_a(string idx) { throw new NotImplementedException(); }

        
    }

    internal class B<T1, T2> : A {
        public static void my_method_b() {}
        readonly static char f = 'f';
        static Random RANDOM = new Random();
        const float pi = 3.14159f;
        public event Action A;
    }

}