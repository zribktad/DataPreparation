using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataPreparation.Data;
using NUnit.Framework;

namespace DataPreparation.Testing
{
    internal interface IPrepareDataAttribute : ITestAction
    {
        protected List<IDataPreparation> TestData { get; set; }
    }
}
