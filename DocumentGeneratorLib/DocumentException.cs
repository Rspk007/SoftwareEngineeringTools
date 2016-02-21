using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftwareEngineeringTools.Documentation
{
    
    public class DocumentException : Exception
    {
        public DocumentException()
        {

        }

        public DocumentException(string message) : base(message)
        {
            
        }

        public DocumentException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
