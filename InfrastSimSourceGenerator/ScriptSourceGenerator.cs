using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace InfrastSimSourceGenerator;

[Generator]
public class ScriptSourceGenerator : ISourceGenerator {
    public void Initialize(GeneratorInitializationContext context) {
        //System.Diagnostics.Debugger.Launch();
    }

    public void Execute(GeneratorExecutionContext context) {
        var targetClassName = "ScriptHelper"; // 指定目标类名
        var targetNamespace = "InfrastSim.TimeDriven"; // 指定目标命名空间
        var semanticModels = context.Compilation.SyntaxTrees
            .Select(tree => context.Compilation.GetSemanticModel(tree));

        var source = new StringBuilder($@"
// <auto-generated/>
using System;
using System.Linq;
using System.Collections.Generic;
using InfrastSim.Localization;

namespace InfrastSim.TimeDriven;

internal static partial class ScriptHelper
{{
    public static readonly Dictionary<string, Action<Simulator, string[]>> MethodMappings = new();
    public static readonly List<string> MethodNames = new();
    public static readonly Dictionary<Language, Dictionary<string, string>> AliasMappings = new();
    static ScriptHelper() {{
");

        var languages = new HashSet<string>();
        foreach (var model in semanticModels) {
            foreach (var classDeclaration in model.SyntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()) {
                var classSymbol = model.GetDeclaredSymbol(classDeclaration);
                if (classSymbol != null && classSymbol.Name == targetClassName && classSymbol.ContainingNamespace.ToDisplayString() == targetNamespace) {
                    foreach (var method in classDeclaration.Members.OfType<MethodDeclarationSyntax>()) {
                        var methodName = method.Identifier.Text;
                        var methodNameLowerCase = methodName.ToLowerInvariant();
                        source.AppendLine($"        MethodMappings.Add(\"{methodNameLowerCase}\", {methodName});");
                        source.AppendLine($"        MethodNames.Add(\"{methodNameLowerCase}\");");

                        var aliasAttrs = method.AttributeLists
                            .SelectMany(attrList => attrList.Attributes)
                            .Where(attr => attr.Name.ToString() == "Alias");
                        foreach (var aliasAttr in aliasAttrs) {
                            var aliasArgs = aliasAttr.ArgumentList.Arguments;
                            if (aliasArgs.Count == 2) {
                                var language = aliasArgs[0].ToString();
                                var aliasName = aliasArgs[1].ToString();
                                if (!languages.Contains(language)) {
                                    languages.Add(language);
                                    source.AppendLine($"        AliasMappings[{language}] = new Dictionary<string, string>();");
                                }
                                source.AppendLine($"        MethodMappings.Add({aliasName}, {methodName});");
                                source.AppendLine($"        AliasMappings[{language}].Add({aliasName}, \"{methodNameLowerCase}\");");
                            }
                        }
                    }
                }
            }
        }

        source.AppendLine("    }");
        source.AppendLine("}");

        context.AddSource("ScriptHelper.g.cs", SourceText.From(source.ToString(), Encoding.UTF8));
    }
}