namespace Domain.Core.Interfaces
{
    public interface ISpecification<T>
    {
        bool IsSatisfiedBy(T entity);
        IQueryable<T> Apply(IQueryable<T> query);
    }

    public abstract class Specification<T> : ISpecification<T>
    {
        public abstract bool IsSatisfiedBy(T entity);
        public virtual IQueryable<T> Apply(IQueryable<T> query)
        {
            return query.Where(e => IsSatisfiedBy(e));
        }
    }
}