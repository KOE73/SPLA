using System;
using System.Reflection;

class Program {
    static void Main() {
        var asm = Assembly.LoadFrom(@"C:\Users\koe\.nuget\packages\markdown.avalonia.syntaxhigh\11.0.3\lib\net6.0\Markdown.Avalonia.SyntaxHigh.dll");
        foreach(var t in asm.GetTypes()) {
            if (t.IsPublic) Console.WriteLine(t.FullName);
        }
    }
}
