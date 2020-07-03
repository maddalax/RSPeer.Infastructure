using System;
using System.Text.Json;

namespace RSPeer.Application.Features.Scripts.Commands.CompileScript.Models
{
    public class CompileException : Exception
    {
        private CompileResult Result { get; }
        public CompileException(CompileResult result)
        {
            Result = result;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(Result);
        }
    }
}