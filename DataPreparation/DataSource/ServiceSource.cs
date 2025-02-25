using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing
{
   //Not for use
    internal class ServiceSource:Attribute,IParameterDataSource
    {
        public IEnumerable GetData(IParameterInfo parameter)
        {

          //  var caseProvider = TestStore.GetRegisteredService(parameter.Method.TypeInfo.Type);


            //yield return caseProvider;
            return null;
        }
    }
}
