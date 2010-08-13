using System;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace PetriNetCore
{
    public static class CollectionExtensions
    {
        public static void Foreach<TInput>(this IEnumerable<TInput> seq, Action<TInput> job)
        {
#if USING_CONTRACTS
            Contract.Requires(seq != (IEnumerable<Tuple<int, string, int>>)null);
#endif
            foreach (var item in seq)
            {
                job(item);
            }
        }
    }

    [Serializable]
    public class ParserException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //
        Errors Errors { get; set; }
        public ParserException(Errors errors)
        {
            Errors = errors;
        }

        public ParserException(string message, Errors errors) : base(message)
        {
            Errors = errors;
        }

        public ParserException(string message,
                          Exception inner) : base(message,
                                                         inner)
        {
        }

        protected ParserException(
            SerializationInfo info,
            StreamingContext context) : base(info,
                                                                          context)
        {
        }
    }
}