// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Text;

namespace Sonnet
{
    public enum ObjectiveSense
    {
        Maximise = -1,
        Minimise = 1
    }

    public class Objective : ModelEntity, IDisposable
    {
        public Objective()
            :base()
        {
            GutsOfConstructor(null, null);
        }
        public Objective(string name)
            :base()
        {
            GutsOfConstructor(name, null);
        }
        public Objective(Expression expr)
            :base()
        {
            Ensure.NotNull(expr, "expression");

            GutsOfConstructor(name, expr);
        }
        public Objective(string name, Expression expr)
            :base()
        {
            Ensure.NotNull(expr, "expression");
            
            GutsOfConstructor(name, expr);
        }

        public static implicit operator Objective(Expression expr)
        {
            return new Objective(expr);
        }

        public override string ToString()
        {
            EnsureNotDisposedOrFinalized();

            return string.Format("Objective {0} : {1}", Name, expression.ToString());
        }

        public virtual string ToLevelString()
        {
            EnsureNotDisposedOrFinalized();

            return string.Format("Objective {0} : {1}", Name, expression.Level());
        }

        public double Value
        {
            get
            {
                EnsureNotDisposedOrFinalized();

                return value;
            }
        }

        public double Level()
        {
            EnsureNotDisposedOrFinalized();

#if (DEBUG)
            if (!Assigned)
            {
                throw new SonnetException("No solver not set!");
            }
#endif
            return expression.Level();
        }

        public double GetCoefficient(Variable var)
        {
            EnsureNotDisposedOrFinalized();
            return expression.GetCoefficient(var);
        }

        public double SetCoefficient(Variable var, double value)
        {
            EnsureNotDisposedOrFinalized();

            double oldValue = expression.SetCoefficient(var, value);
            foreach(Solver solver in solvers) solver.SetObjectiveCoefficient(var, value);
            return oldValue;
        }

        internal void Assemble()
        {
            EnsureNotDisposedOrFinalized();

            expression.Assemble();
        }

        internal void Assign(Solver solver, double value)
        {
            EnsureNotDisposedOrFinalized();

            base.Assign(solver, -1);
            this.value = value;
        }

        internal CoefVector Coefficients
        {
            get
            {
                EnsureNotDisposedOrFinalized();
                return expression.Coefficients;
            }
        }

        internal double Constant
        {
            get
            {
                EnsureNotDisposedOrFinalized();
                return expression.Constant;
            }
        }

        private void GutsOfConstructor(string name, Expression expr)
        {
            this.id = numberOfObjectives++;

            if (!object.ReferenceEquals(expr, null)) this.expression = new Expression(expr);
            else this.expression = new Expression();

            // get rid of the constrant
            //this.expression.Subtract(this.expression.constant);

            if (name != null) Name = name;
            else Name = string.Format("Obj[{0}]", id);
        }

        private static int numberOfObjectives = 0;
        private Expression expression;
        private double value;

        private void EnsureNotDisposedOrFinalized()
        {
            if (disposed > 0)// || finalized > 0)
            {
                string message = string.Format("Cannot use Objective after it was disposed {0} times.", disposed);
                throw new ObjectDisposedException("Objective", message);
            }
        }

        private int disposed;

        #region IDisposable Members

        public void Dispose()
        {
            if (disposed > 0) return;

            if (!object.ReferenceEquals(expression, null))
            {
                expression.Dispose();
                expression = null;
            }

            disposed++;
        }

        #endregion
    }
}
