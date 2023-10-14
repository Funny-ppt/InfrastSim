using InfrastSim.TimeDriven.Operators;
using System.Diagnostics;
using System.Linq.Expressions;

namespace InfrastSim.TimeDriven;
internal class OperatorBuilder {

    Expression? _resolveExpr;
    List<Expression<Action<OperatorBase, TimeDrivenSimulator>>> _actionGroup = new();
    readonly ParameterExpression _opParam = Expression.Parameter(typeof(OperatorBase), "op");
    readonly ParameterExpression _simuParam = Expression.Parameter(typeof(TimeDrivenSimulator), "simu");

    OperatorBuilder If(Expression<Func<OperatorBase, TimeDrivenSimulator, bool>> condition) {
        if (_resolveExpr == null) {
            _resolveExpr = Expression.Block(
                _actionGroup.AsEnumerable<Expression>().Append(
                    Expression.IfThen(
                        Expression.Invoke(condition, _opParam, _simuParam),
                        ReplaceVisitor.PlaceHolder
                ))
            );
        } else {
            var cond_expr = Expression.IfThen(
                Expression.Invoke(condition, _opParam, _simuParam),
                ReplaceVisitor.PlaceHolder
            );
            var block = Expression.Block(_actionGroup.AsEnumerable<Expression>().Append(cond_expr));
            _resolveExpr = new ReplaceVisitor(block).Visit(_resolveExpr);
        }
        return this;
    }

    OperatorBuilder Do(Expression<Action<OperatorBase, TimeDrivenSimulator>> action) {
        _actionGroup.Add(action);
        return this;
    }

    public void Bind(OperatorBase op) {
        if (_resolveExpr != null) {
            var block = Expression.Block(_actionGroup);
            var expr = new ReplaceVisitor(block).Visit(_resolveExpr);

            Debug.Assert(expr != null);
            var lambda = Expression.Lambda<Action<OperatorBase, TimeDrivenSimulator>>(
                expr, _opParam, _simuParam
            ).Compile();
            op.OnResolve = simu => lambda(op, simu);
        }
    }

    public OperatorBuilder RequiredUpgraded(int upgraded) {
        If((op, simu) => op.Upgraded >= upgraded);
        return this;
    }
    public OperatorBuilder In(FacilityType type) {
        If((op, simu) => op.Facility != null && op.Facility.Type == type && !op.IsTired);
        return this;
    }
    public OperatorBuilder WithGroupMemberInSameFacility(string group) {
        If((op, simu) => op.Facility != null && op.Facility.HasGroupMember(group));
        return this;
    }
    public OperatorBuilder SetGlobalValueIfGreater(string name, string tag, double value) {
        Do((op, simu) => simu.GetGlobalValue(name).SetIfGreater(tag, value));
        return this;
    }
    public OperatorBuilder ModifyValue(
        Expression<Func<OperatorBase, TimeDrivenSimulator, AggregateValue>> valueSelector,
        Expression<Action<AggregateValue>> valueHandler) {
        return this;
    }
}

