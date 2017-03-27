using Microsoft.VisualStudio.TestTools.UnitTesting;
using GroundController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroundController.Tests {
    [TestClass()]
    public class InputAdapterTests {
        [TestMethod()]
        public void OpenTest() {

            InputAdapter input = new InputAdapter();

            input.Read("build.3ds");
        }
    }
}