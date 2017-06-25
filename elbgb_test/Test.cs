﻿using System.Xml.Serialization;

namespace elbgb_test
{
    public class Test
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public int FrameCount { get; set; }

        [XmlAttribute]
        public string Hash { get; set; }

        [XmlAttribute]
        public TestStatus Status { get; set; }

        [XmlIgnore]
        public TestStatus Result { get; set; }
    }
}
