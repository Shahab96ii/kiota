using System.Linq;
using Xunit;

namespace Kiota.Builder.Refiners.Tests {
    public class RubyLanguageRefinerTests {

        private readonly CodeNamespace graphNS;
        private readonly CodeClass parentClass;
        private readonly CodeNamespace root = CodeNamespace.InitRootNamespace();
        public RubyLanguageRefinerTests() {
            root = CodeNamespace.InitRootNamespace();
            graphNS = root.AddNamespace("graph");
            parentClass = new () {
                Name = "parentClass"
            };
            graphNS.AddClass(parentClass);
        }
        #region CommonLanguageRefinerTests
        [Fact]
        public void AddsDefaultImports() {
            var model = root.AddClass(new CodeClass {
                Name = "model",
                ClassKind = CodeClassKind.Model
            }).First();
            var requestBuilder = root.AddClass(new CodeClass {
                Name = "rb",
                ClassKind = CodeClassKind.RequestBuilder,
            }).First();
            ILanguageRefiner.Refine(new GenerationConfiguration { Language = GenerationLanguage.Ruby, ClientNamespaceName = graphNS.Name }, root);
            Assert.NotEmpty(model.StartBlock.Usings);
            Assert.NotEmpty(requestBuilder.StartBlock.Usings);
        }
        #endregion
        #region RubyLanguageRefinerTests
        [Fact]
        public void EscapesReservedKeywords() {
            var model = root.AddClass(new CodeClass {
                Name = "break",
                ClassKind = CodeClassKind.Model
            }).First();
            ILanguageRefiner.Refine(new GenerationConfiguration { Language = GenerationLanguage.Ruby, ClientNamespaceName = graphNS.Name }, root);
            Assert.NotEqual("break", model.Name);
            Assert.Contains("escaped", model.Name);
        }
        [Fact]
        public void AddInheritedAndMethodTypesImports() {
            var model = root.AddClass(new CodeClass {
                Name = "model",
                ClassKind = CodeClassKind.Model
            }).First();
            var declaration = model.StartBlock as CodeClass.Declaration;
            declaration.Inherits = new (){
                Name = "someInterface"
            };
            ILanguageRefiner.Refine(new GenerationConfiguration { Language = GenerationLanguage.Ruby, ClientNamespaceName = graphNS.Name }, root);
            Assert.Equal("someInterface", declaration.Usings.First().Declaration.Name);
        }
        [Fact]
        public void FixInheritedEntityType() {
            var model = root.AddClass(new CodeClass {
                Name = "model",
                ClassKind = CodeClassKind.Model
            }).First();
            var entity = graphNS.AddClass(new CodeClass {
                Name = "entity",
                ClassKind = CodeClassKind.Model
            }).First();
            var declaration = model.StartBlock as CodeClass.Declaration;
            declaration.Inherits = new (){
                Name = "entity"
            };
            ILanguageRefiner.Refine(new GenerationConfiguration { Language = GenerationLanguage.Ruby, ClientNamespaceName = graphNS.Name }, root);
            Assert.Equal("Graph::Entity", declaration.Inherits.Name);
        }
        [Fact]
        public void AddNamespaceModuleImports() {
            var declaration = parentClass.StartBlock as CodeClass.Declaration;
            var subNS = graphNS.AddNamespace($"{graphNS.Name}.messages");
            var messageClassDef = new CodeClass {
                Name = "Message",
            };
            subNS.AddClass(messageClassDef);
            declaration.AddUsings(new CodeUsing() {
                Name = messageClassDef.Name,
                Declaration = new() {
                    Name = messageClassDef.Name,
                    TypeDefinition = messageClassDef,
                }
            });
            ILanguageRefiner.Refine(new GenerationConfiguration { Language = GenerationLanguage.Ruby, ClientNamespaceName = graphNS.Name }, root);
            Assert.Equal("Message", declaration.Usings.First().Declaration.Name);
            Assert.Equal("./graph", declaration.Usings.Last().Declaration.Name);
        }
        #endregion
    }
}
