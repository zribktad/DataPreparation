using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing.Attributes
{
    public class PrepareClassData : Attribute, ITestAction
    {
        private readonly string _className;
        public PrepareClassData(string className = null)
        {
            _className = className ?? "UnknownClass"; 
        }


        public void BeforeTest(ITest test)
        {
            Console.WriteLine("Automatically loading attributes...");

            // Tu môžeš implementovať logiku na načítanie všetkých atribútov pomocou reflexie
            var allTypes = test.Fixture.GetType().Assembly.GetTypes();

            foreach (var type in allTypes)
            {
                var methods = type.GetMethods(
                                    System.Reflection.BindingFlags.Public |
                                              System.Reflection.BindingFlags.NonPublic |
                                              System.Reflection.BindingFlags.Instance |
                                              System.Reflection.BindingFlags.Static
                                    );

                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(false);
                    if (attributes.Length > 0)
                    {
                        Console.WriteLine($"Method {method.Name} in class {type.Name} has attributes:");
                        foreach (var attr in attributes)
                        {
                            Console.WriteLine($"- {attr.GetType().Name}");
                        }
                    }
                }
            }
        }

        public void AfterTest(ITest test)
        {
          
        }

        public ActionTargets Targets => ActionTargets.Test;
    }
}
