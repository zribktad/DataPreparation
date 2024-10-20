using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing.Attributes
{
    public class UsePreparedDataForAttribute: Attribute, ITestAction
    {

        public UsePreparedDataForAttribute(Type classType, params string[] methodsNames)
        {
        }

        public void BeforeTest(ITest test)
        {
          
        }

        public void AfterTest(ITest test)
        {
           
        }

        public ActionTargets Targets => ActionTargets.Test;
    }
}
