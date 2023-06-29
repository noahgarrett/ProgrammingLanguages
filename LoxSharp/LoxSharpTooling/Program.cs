using System.Text;

namespace LoxSharpTooling;

/// <summary>
/// Generate AST
/// </summary>
internal class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: generate_ast <output directory>");
            Environment.Exit(1);
        }

        string outputDir = args[0];

        defineAST(outputDir, "Expr", new List<string>()
        {
            "Assign     : Token name, Expr value",
            "Binary     : Expr left, Token oper, Expr right",
            "Call       : Expr callee, Token paren, List<Expr> arguments",
            "Get        : Expr obj, Token name",
            "Grouping   : Expr expression",
            "Literal    : object value",
            "Logical    : Expr left, Token oper, Expr right",
            "Set        : Expr obj, Token name, Expr value",
            "This       : Token keyword",
            "Unary      : Token oper, Expr right",
            "Variable   : Token name"
        });

        defineAST(outputDir, "Stmt", new List<string>()
        {
            "Block          : List<Stmt> statements",
            "Class          : Token name, List<Stmt.Function> methods",
            "Expression     : Expr expression",
            "Function       : Token name, List<Token> parameters, List<Stmt> body",
            "If             : Expr condition, Stmt thenBranch, Stmt elseBranch",
            "Print          : Expr expression",
            "Return         : Token keyword, Expr value",
            "Var            : Token name, Expr intitializer",
            "While          : Expr condition, Stmt body"
        });
    }

    private static void defineAST(string outputDir, string baseName, List<string> types)
    {
        string path = Path.Combine(outputDir, baseName + ".cs");

        using StreamWriter writer = new(path, false, Encoding.UTF8);

        writer.WriteLine("namespace LoxSharp;");
        writer.WriteLine();
        writer.WriteLine("using LoxSharp.Models;");
        writer.WriteLine();
        writer.WriteLine($"public abstract class {baseName}");
        writer.WriteLine("{");

        defineVisitor(writer, baseName, types);

        foreach (string type in types)
        {
            string className = type.Split(":")[0].Trim();
            string fields = type.Split(":")[1].Trim();
            defineType(writer, baseName, className, fields);
        }

        writer.WriteLine();
        writer.WriteLine("  public abstract T accept<T>(Visitor<T> visitor);");

        writer.WriteLine("}");
        writer.Close();
    }

    private static void defineVisitor(StreamWriter writer, string baseName, List<string> types)
    {
        writer.WriteLine("  public interface Visitor<T> {");

        foreach (string type in types)
        {
            string typeName = type.Split(":")[0].Trim();
            writer.WriteLine($"     T visit{typeName}{baseName}({typeName} {baseName.ToLower()});");
        }

        writer.WriteLine("  }");
    }

    private static void defineType(StreamWriter writer, string baseName, string className, string fieldList)
    {
        writer.WriteLine($"  public class {className} : {baseName} " + "{");

        // Constructor
        writer.WriteLine($"      public {className}({fieldList}) " + "{");

        // Store parameters in fields
        List<string> fields = fieldList.Split(", ").ToList();
        foreach (string field in fields)
        {
            string name = field.Split(" ")[1];
            writer.WriteLine($"         this.{name} = {name};");
        }

        writer.WriteLine("  }");

        // Visitor Pattern
        writer.WriteLine();
        writer.WriteLine("      public override T accept<T>(Visitor<T> visitor) {");
        writer.WriteLine($"          return visitor.visit{className}{baseName}(this);");
        writer.WriteLine("      }");

        // Fields
        writer.WriteLine();

        foreach (string field in fields)
        {
            writer.WriteLine($"     public readonly {field};");
        }

        writer.WriteLine("  }");
    }
}