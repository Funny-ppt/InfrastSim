using System.Diagnostics;
using System.Linq.Expressions;

namespace InfrastSim.TimeDriven;
internal class OperatorSkillBuilder {

    Expression? _resolveExpr;
    List<Expression<Action<OperatorBase, Simulator>>> _actionGroup = new();
    readonly ParameterExpression _opParam = Expression.Parameter(typeof(OperatorBase), "op");
    readonly ParameterExpression _simuParam = Expression.Parameter(typeof(Simulator), "simu");

    OperatorSkillBuilder If(Expression<Func<OperatorBase, Simulator, bool>> condition) {
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

    OperatorSkillBuilder Do(Expression<Action<OperatorBase, Simulator>> action) {
        _actionGroup.Add(action);
        return this;
    }

    public void Bind(OperatorBase op) {
        if (_resolveExpr != null) {
            var block = Expression.Block(_actionGroup);
            var expr = new ReplaceVisitor(block).Visit(_resolveExpr);

            Debug.Assert(expr != null);
            var lambda = Expression.Lambda<Action<OperatorBase, Simulator>>(
                expr, _opParam, _simuParam
            ).Compile();
            op.OnResolve = simu => lambda(op, simu);
        }
    }

    public OperatorSkillBuilder RequiredUpgraded(int upgraded) {
        If((op, simu) => op.Upgraded >= upgraded);
        return this;
    }
    public OperatorSkillBuilder In(FacilityType type) {
        If((op, simu) => op.Facility != null && op.Facility.Type == type && !op.IsTired);
        return this;
    }
    public OperatorSkillBuilder WithGroupMemberInSameFacility(string group) {
        If((op, simu) => op.Facility != null && op.Facility.HasGroupMember(group));
        return this;
    }
    public OperatorSkillBuilder SetGlobalValueIfGreater(string name, string tag, double value) {
        Do((op, simu) => simu.GetGlobalValue(name).SetIfGreater(tag, value));
        return this;
    }
    public OperatorSkillBuilder SetValue(AggregateValue aggregateValue, string tag, double value) {
        Do((op, simu) => aggregateValue.SetValue(tag, value));
        return this;
    }
    public OperatorSkillBuilder SetValueIfGreater(AggregateValue aggregateValue, string tag, double value) {
        Do((op, simu) => aggregateValue.SetIfGreater(tag, value));
        return this;
    }
    public OperatorSkillBuilder SetValueIfLesser(AggregateValue aggregateValue, string tag, double value) {
        Do((op, simu) => aggregateValue.SetIfLesser(tag, value));
        return this;
    }
}

