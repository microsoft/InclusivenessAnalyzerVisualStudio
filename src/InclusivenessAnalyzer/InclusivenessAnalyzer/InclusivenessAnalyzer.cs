using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace InclusivenessAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InclusivenessAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "Inclusive";
        private static readonly Dictionary<string, string> InclusiveTerms = new Dictionary<string, string>() {
            {"whitelist", "allow list, access list, permit" },
            {"white list", "allow list, access list, permit" },
            {"blacklist", "deny list, blocklist, exclude list" },
            {"black list", "deny list, blocklist, exclude list" },
            {"master", "primary, main, default, leader" },
            {"slave", "replica, standby, secondary, follower" },
            {"minority", "marginalized groups, underrepresented groups, people of color" },
            {"minorities", "marginalized groups, underrepresented groups, people of color" },
            {"brownbag", "learning session, lunch-and-learn, sack lunch" },
            {"brown bag", "learning session, lunch-and-learn, sack lunch" },
            {"white box", "open-box" },
            {"white-box", "open-box" },
            {"black box", "closed-box" },
            {"black-box", "closed-box" },
            {"culture fit", "values fit, cultural contribution" },
            {"citizen", "resident, people" },
            {"guys", "folks, you all, y'all, people, teammates, team" },
            {"he", "they" },
            {"his", "their, theirs" },
            {"him", "them" },
            {"she", "they" },
            {"her", "their" },
            {"hers", "theirs" },
            {"manpower", "human effort, person hours, engineer hours, work, workforce, personnel, team, workers" },
            {"man hours", "human effort, person hours, engineer hours, work, workforce, personnel, team, workers" },
            {"mankind", "people, humanity" },
            {"chairman", "chairperson, spokesperson, moderator, discussion leader, chair" },
            {"foreman", "chairperson, spokesperson, moderator, discussion leader, chair" },
            {"middleman", "middle person, intermediary, agent, dealer" },
            {"mother", "parent, caretaker, nurturer, guardian" },
            {"mothering", "parenting, caretaking, caring, nurturing" },
            {"father", "parent, caretaker, nurturer, guardian" },
            {"fathering", "parenting, caretaking, caring, nurturing" },
            {"wife", "spouse, partner, significant other" },
            {"husband", "spouse, partner, significant other" },
            {"boyfriend", "partner, significant other" },
            {"girlfriend", "partner, significant other" },
            {"girl", "woman" },
            {"girls", "women" },
            {"female", "woman" },
            {"females", "women" },
            {"boy", "man" },
            {"boys", "men" },
            {"male", "man" },
            {"males", "men" },
            {"mom test", "user test" },
            {"girlfriend test", "user test" },
            {"ninja", "professional" },
            {"rockstar", "professional" },
            {"housekeeping", "maintenance, cleanup, preparation" },
            {"opposite sex", "different sex" },
            {"grandfathered in", "exempt" },
            {"grandfathered", "exempt" },
            {"midget", "little person, short stature, person with dwarfism" },
            {"crazy", "unexpected, unpredictable, surprising" },
            {"insane", "unexpected, unpredictable, surprising" },
            {"freak", "unexpected, unpredictable, surprising" },
            {"tone deaf", "oblivious" },
            {"blind spot", "dead spot, unseen area" },
            {"OCD", "organized, detail-oriented" },
            {"depressed", "sad, upset" },
            {"depressing", "saddening, upsetting" },
            {"handicap", "person with a disability" },
            {"cripple", "person with a disability" },
            {"sanity check", "quick check, confidence check, coherence check" },
            {"sane", "correct, adequate, sufficient, valid, sensible, coherent" },
            {"retard", "person with disabilities, mentally limited" },
            {"dummy value", "placeholder value, sample value, design value" }
        };

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string HelpLinkUri = "https://github.com/merill/InclusivenessAnalyzer/";
        private const string Category = "Microsoft.Inclusive";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description, helpLinkUri: HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            try
            {
                context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
                context.EnableConcurrentExecution();

                context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType, SymbolKind.Method, SymbolKind.Property, SymbolKind.Field,
                    SymbolKind.Event, SymbolKind.Namespace, SymbolKind.Parameter);

                context.RegisterSyntaxNodeAction(CheckComments, SyntaxKind.VariableDeclaration, SyntaxKind.CatchDeclaration, SyntaxKind.NamespaceDeclaration,
                    SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.InterfaceDeclaration, SyntaxKind.EnumDeclaration, SyntaxKind.DelegateDeclaration,
                    SyntaxKind.EnumMemberDeclaration, SyntaxKind.FieldDeclaration, SyntaxKind.EventFieldDeclaration, SyntaxKind.MethodDeclaration, SyntaxKind.OperatorDeclaration,
                    SyntaxKind.ConversionOperatorDeclaration, SyntaxKind.ConstructorDeclaration, SyntaxKind.DestructorDeclaration, SyntaxKind.PropertyDeclaration,
                    SyntaxKind.EventDeclaration, SyntaxKind.IndexerDeclaration, SyntaxKind.GetAccessorDeclaration, SyntaxKind.SetAccessorDeclaration,
                    SyntaxKind.AddAccessorDeclaration, SyntaxKind.RemoveAccessorDeclaration, SyntaxKind.UnknownAccessorDeclaration);
            }
            catch { }
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            try
            {
                var symbol = context.Symbol;

                // Find just those named type symbols with non-inclusive terms.
                foreach (KeyValuePair<string, string> entry in InclusiveTerms)
                {
                    if (IsMatch(symbol.Name, entry.Key))
                    {
                        // For all such symbols, produce a diagnostic.
                        var diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], symbol.Name, entry.Value);

                        context.ReportDiagnostic(diagnostic);
                        break;
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Checks if term was found in the symbol.
        /// Match whole word if it has less than 5 characters
        /// </summary>
        /// <param name="symbol">The phrase that is being checked.</param>
        /// <param name="term">The non-inclusive phrase we are looking for.</param>
        /// <returns></returns>
        private static bool IsMatch(string symbol, string term)
        {
            return term.Length < 5 ?
                symbol.Equals(term, StringComparison.InvariantCultureIgnoreCase) :
                symbol.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0;
            
        }

        private void CheckComments(SyntaxNodeAnalysisContext context)
        {
            try
            {
                var node = context.Node;

                var xmlTrivia = node.GetLeadingTrivia()
                    .Select(i => i.GetStructure())
                    .OfType<DocumentationCommentTriviaSyntax>()
                    .FirstOrDefault();

                if (xmlTrivia == null) return;

                var content = xmlTrivia.ToFullString();
                foreach (KeyValuePair<string, string> entry in InclusiveTerms)
                {
                    if (IsMatch(content, entry.Key))
                    {
                        // For all such symbols, produce a diagnostic.
                        var diagnostic = Diagnostic.Create(Rule, xmlTrivia.GetLocation(), entry.Key, entry.Value);

                        context.ReportDiagnostic(diagnostic);
                        break;
                    }
                }
            }
            catch { }
        }
    }
}