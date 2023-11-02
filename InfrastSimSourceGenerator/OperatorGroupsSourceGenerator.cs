using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace InfrastSimSourceGenerator; 

[Generator]
public class OperatorGroupsSourceGenerator : ISourceGenerator {
    public void Initialize(GeneratorInitializationContext context) {
        //Debugger.Launch();
        context.RegisterForSyntaxNotifications(() => new OperatorGroupsSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context) {
        if (context.SyntaxReceiver is not OperatorGroupsSyntaxReceiver receiver)
            return;

        var sourceBuilder = new StringBuilder(@"
using System.Linq;
using System.Collections.Frozen;

namespace InfrastSim.TimeDriven;

internal static partial class OperatorGroups
{
    static OperatorGroups()
    {
        var groups = new Dictionary<string, HashSet<string>>()
        {");

        foreach (var fieldName in receiver.FieldNames) {
            sourceBuilder.AppendLine($@"
            {{ ""{fieldName}"", {fieldName} }},");
        }

        sourceBuilder.Append(@"
        };
        Groups = groups.ToFrozenDictionary();
    }

    public static readonly FrozenDictionary<string, HashSet<string>> Groups;
}");

        var source = sourceBuilder.ToString();
        context.AddSource("OperatorGroups.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    class OperatorGroupsSyntaxReceiver : ISyntaxReceiver {
        public HashSet<string> FieldNames { get; } = new HashSet<string>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode) {
            if (syntaxNode is ClassDeclarationSyntax classDeclaration &&
                classDeclaration.Identifier.Text == "OperatorGroups") {
                foreach (var member in classDeclaration.Members) {
                    if (member is FieldDeclarationSyntax fieldDeclaration &&
                        fieldDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword) &&
                        fieldDeclaration.Modifiers.Any(SyntaxKind.ReadOnlyKeyword) &&
                        fieldDeclaration.Declaration.Type is GenericNameSyntax genericName &&
                        genericName.Identifier.Text == "HashSet" &&
                        genericName.TypeArgumentList.Arguments.Count == 1 &&
                        genericName.TypeArgumentList.Arguments[0].ToString() == "string") {
                        var variable = fieldDeclaration.Declaration.Variables.FirstOrDefault();
                        if (variable != null) {
                            FieldNames.Add(variable.Identifier.Text);
                        }
                    }
                }
            }
        }
    }
}
