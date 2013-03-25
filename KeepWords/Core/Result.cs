using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KeepWords.Core.Web;

namespace KeepWords.Core
{
    public interface IResult
    {
        int Code { get; }
        string Message { get; }
    }

    [Serializable]
    public class Result : IResult
    {
        public static readonly IResult Success = new Result { Code = (int)ResultCodes.Success, Message = Messages.OK };
        public int Code { get; set; }
        public string Message { get; set; }
                
        public static Result<T> Generic<T>(IResult resultToWrap, T data)
        {
            return new Result<T> { Code = resultToWrap.Code, Message = resultToWrap.Message, Data = data };
        }
    }

    [Serializable]
    public class Result<T> : Result
    {
        public T Data { get; set; }
    }

    public enum ResultCodes
    {
        FatalError = -1001,
        UserError = 0,
        Success = 1
    }

}