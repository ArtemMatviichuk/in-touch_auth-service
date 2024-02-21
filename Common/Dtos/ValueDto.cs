using System.ComponentModel.DataAnnotations;

namespace AuthService.Common.Dtos
{
    public class ValueDto<T>
    {
        public ValueDto(T value)
        {
            Value = value;
        }

        public T? Value { get; set; }
    }
}
