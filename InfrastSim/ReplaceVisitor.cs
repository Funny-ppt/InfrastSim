using System.Linq.Expressions;

namespace InfrastSim;
public class ReplaceVisitor : ExpressionVisitor {
    public readonly static Expression PlaceHolder = Expression.Constant(null);
    private readonly Expression _newExpression;

    public ReplaceVisitor(Expression newExpression) {
        _newExpression = newExpression ?? throw new ArgumentNullException(nameof(newExpression));
    }

    public override Expression? Visit(Expression? node) {
        if (ReferenceEquals(node, PlaceHolder))
            return _newExpression;

        return base.Visit(node);
    }
}
