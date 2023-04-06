using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CSToMD {

    public static class CSToMD {

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

}