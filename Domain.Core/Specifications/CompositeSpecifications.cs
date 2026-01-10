namespace Domain.Core.Specifications
{
    public class AndSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _left;
        private readonly ISpecification<T> _right;

        public AndSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override bool IsSatisfiedBy(T entity)
        {
            return _left.IsSatisfiedBy(entity) && _right.IsSatisfiedBy(entity);
        }
    }
    public class OrSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _left;
        private readonly ISpecification<T> _right;

        public OrSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override bool IsSatisfiedBy(T entity)
        {
            return _left.IsSatisfiedBy(entity) || _right.IsSatisfiedBy(entity);
        }
    }
    public static class SpecificationExtensions
    {
        public static ISpecification<T> And<T>(this ISpecification<T> left, ISpecification<T> right)
            => new AndSpecification<T>(left, right);

        public static ISpecification<T> Or<T>(this ISpecification<T> left, ISpecification<T> right)
            => new OrSpecification<T>(left, right);
    }
}