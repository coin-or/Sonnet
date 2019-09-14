// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

//#define SONNET_SETWARMROWPRICE
using System;
using System.Collections.Generic;
using System.Text;

using COIN;

namespace Sonnet
{
    /// <summary>
    /// The class WarmStart can be used to warm-start another optimization.
    /// The WarmStart object should be taken from the solver (GetWarmStart) just after a Solve(),
    /// and applied to the solver (SetWarmStart) before another solve.
    /// Use via solver.GetWarmStart() and solver.SetWarmStart().
    /// The model at the solver that called GetWarmStart should not be significantly different from 
    /// the model at the solver of the subsequent SetWarmStart.
    /// </summary>
    public class WarmStart : IDisposable
    {
        internal static WarmStart NewWarmStart(OsiSolverInterface solver)
        {
            unsafe
            {
#if (SONNET_SETWARMROWPRICE)
                return new WarmStart(solver.getWarmStart(), solver.getNumCols(), solver.getColSolutionUnsafe(), solver.getNumCols(), solver.getRowPriceUnsafe());
#else
                return new WarmStart(solver.getWarmStart(), solver.getNumCols(), solver.getColSolutionUnsafe(), 0, null);
#endif
            }
        }
        
        internal static WarmStart NewEmptyWarmStart(OsiSolverInterface solver)
        {
            unsafe
            {
                return new WarmStart(solver.getEmptyWarmStart(), 0, null, 0, null);
            }
        }
        
        internal void ApplyWarmStart(OsiSolverInterface solver)
        {
            Ensure.NotNull(solver, "solver");

            unsafe
            {
                // set the primal solution
                if (numberColumns > 0)
                {
                    // if the new solver has *fewer* columns, then no problem.
                    // if the new solver has MORE columns, then make sure we dont copy wrong memory..
                    if (solver.getNumCols() > numberColumns)
                    {
                        int n = solver.getNumCols();

                        double[] newColSolution = new double[n];
                        fixed (double* newColSolutionPinned = newColSolution)
                        {
                            // copy the first NumberOfColumns into the new array
                            CoinUtils.CoinDisjointCopyN(colSolution, numberColumns, newColSolutionPinned);
                            // then fill it up with zeros
                            CoinUtils.CoinZeroN(&newColSolutionPinned[numberColumns], (n - numberColumns));
                        }
                        solver.setColSolution(newColSolution);
                    }
                    else
                    {
                        solver.setColSolutionUnsafe(colSolution);
                    }

                    // set the dual solution
                    if (numberRows > 0)
                    {
                        // if the new solver has *fewer* rows, then no problem.
                        // if the new solver has MORE rows, then make sure we dont copy wrong memory..
                        if (solver.getNumRows() > numberRows)
                        {
                            int m = solver.getNumRows();

                            double[] newRowPrice = new double[m];
                            fixed (double* newRowPricePinned = newRowPrice)
                            {
                                // copy the first NumberOfRows into the new array
                                CoinUtils.CoinDisjointCopyN(rowPrice, numberRows, newRowPricePinned);
                                // then fill it up with zeros
                                CoinUtils.CoinZeroN(&newRowPricePinned[numberRows], (m - numberRows));
                            }
                            solver.setRowPrice(newRowPrice);
                        }
                        else
                        {
                            solver.setRowPriceUnsafe(rowPrice);
                        }
                    }
                }
#if (!SONNET_SETWARMROWPRICE)
        		if (numberRows > 0 || rowPrice != null) throw new NotSupportedException();
#endif
            }

            // set the warmstart object
            solver.setWarmStart(coinWarmStart);
        }

        private WarmStart() { throw new System.NotSupportedException(); }
        private unsafe WarmStart(CoinWarmStart coinWarmStart, int numberColumns, double* colSolution, int numberRows, double* rowPrice)
        {
            this.coinWarmStart = coinWarmStart;
            this.numberColumns = numberColumns;
            this.numberRows = numberRows;

            if (numberColumns > 0)
            {
                this.colSolution = CoinUtils.NewDoubleArray(numberColumns);
                CoinUtils.CoinDisjointCopyN(colSolution, numberColumns, this.colSolution);
            }

            if (numberRows > 0)
            {
                this.rowPrice = CoinUtils.NewDoubleArray(numberRows);
                CoinUtils.CoinDisjointCopyN(rowPrice, numberRows, this.rowPrice);
            }
        }

        private CoinWarmStart coinWarmStart;
        private unsafe double* colSolution;
        private int numberColumns;
        private unsafe double* rowPrice;
        private int numberRows;

        #region IDisposable Members
        // See also http://msdn.microsoft.com/en-us/library/system.idisposable.aspx

        /// <summary>
        /// Releases all resources used by the WarmStart.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the WarmStart
        /// and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                    coinWarmStart.Dispose();
                }

                // Free your own state (unmanaged objects).
                // Set large fields to null.
                unsafe
                {
                    CoinUtils.DeleteArray(this.colSolution);
                    this.colSolution = null;

                    CoinUtils.DeleteArray(this.rowPrice);
                    this.rowPrice = null;
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Use C# destructor syntax for finalization code.
        /// </summary>
        ~WarmStart()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

        private bool disposed = false;
        #endregion
    }
}
