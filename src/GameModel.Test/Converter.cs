using System;
using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace GameModel.Test {
    public class Converter : TextWriter
    {
        ITestOutputHelper _output;
        public Converter(ITestOutputHelper output) {
            _output = output;
        }
        public override Encoding Encoding {
            get { return Encoding.UTF8; }
        }
        public override void WriteLine(string message) {
            _output.WriteLine(message);
        }
        public override void WriteLine(string format, params object[] args) {
            _output.WriteLine(format, args);
        }

        public override void Write(char value) {
            throw new NotSupportedException("This text writer only supports WriteLine(string) and WriteLine(string, params object[]).");
        }
    }
}