using System;

namespace Origam.ErrorHandling
{
    public class ResultEnum
    {
        public bool IsSuccess { get; }
        public ErrType? Error { get; }
        public bool IsFailure => !IsSuccess;

        protected ResultEnum(bool isSuccess, ErrType? error)
        {
            if (isSuccess && error != null)
                throw new InvalidOperationException();
            if (!isSuccess && error == null)
                throw new InvalidOperationException();

            IsSuccess = isSuccess;
            Error = error;
        }

        public static ResultEnum Fail(ErrType? error)
        {
            return new ResultEnum(false, error);
        }

        public static ResultEnum<T> Fail<T>(ErrType? error)
        {
            return new ResultEnum<T>(default(T), false, error);
        }

        public static ResultEnum Ok()
        {
            return new ResultEnum(true, null);
        }

        public static ResultEnum<T> Ok<T>(T value)
        {
            return new ResultEnum<T>(value, true, null);
        }

        public static ResultEnum Combine(params ResultEnum[] results)
        {
            foreach (ResultEnum result in results)
            {
                if (result.IsFailure)
                    return result;
            }

            return Ok();
        }
    }


    public class ResultEnum<T> : ResultEnum
    {
        private readonly T _value;
        public T Value
        {
            get
            {
                if (!IsSuccess)
                    throw new InvalidOperationException();

                return _value;
            }
        }

        protected internal ResultEnum(T value, bool isSuccess, ErrType? error)
            : base(isSuccess, error)
        {
            _value = value;
        }
    }
}
