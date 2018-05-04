using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fingerprint
{
    public class MyPerson : SourceAFIS.Simple.Person
    {
        public string Name { get; set; }
        public string FingerUrl { get; set; }
        public float MatchValue { get; set; }
    }

    public class FingerprintObject : SourceAFIS.Simple.Fingerprint
    {
        public string FileName { get; set; }
    }
}
