// SonnetWrapperTest.cpp : main project file.

#include <OsiClpSolverInterface.hpp>
#include <OsiCbcSolverInterface.hpp>
#include <ClpSimplex.hpp>
#include <CbcModel.hpp>
#include <CbcSolver.hpp>
using namespace System;

void TestQuad1()
{
  // See https://www.inverseproblem.co.nz/OPTI/index.php/Probs/MIQP
  //  min 0.5 x1^2 + x2^2 - x1x2 - 2x1 - 6x2
  //  st. x1 + x2 <= 2
  //    -x1 + 2x2 <= 2
  //     2x1 + x2 <= 3
  //      x1 >= 0, x2 >= 0
  //     x1 and x2 integer
  //
  // Linear relaxation: x1 = 0.66, x2 = 1.333  -> obj = -8.22222
  // Integer solution:  x1 = 1,    x2 = 1      -> obj = -7.5

  int numcols = 2;
  int numrows = 3;

  // The main problem
  int startA[3] = { 0, 3, 6 }; // per variable (column), the starting position of its nonzero data, plus final
  int indexA[6] = { 0, 1, 2, 0, 1, 2 }; // The constraint index number per nonzero element
  double valueA[6] = { 1.0, -1.0, 2.0, 1.0, 2.0, 1.0 }; // The nonzero elements
  double collb[2] = { 0.0, 0.0 };
  double colub[2] = { DBL_MAX, DBL_MAX };
  double obj[2] = { -2.0, -6.0 };
  double rowlb[3] = { DBL_MIN, DBL_MIN, DBL_MIN };
  double rowub[3] = { 2.0, 2.0, 3.0 };
  // Quadratic objective
  int startObj[3] = { 0, 2, 3 };
  int columnObj[3] = { 0, 1, 1 };
  double elementObj[3] = { 1.0, -1.0, 2.0 }; // diagonals get double coefs, non-diagonals normal coefs values

  OsiClpSolverInterface osiClp;
  osiClp.loadProblem(numcols, numrows, startA, indexA, valueA, collb, colub, obj, rowlb, rowub);
  osiClp.setRowName(0, "row1");
  osiClp.setRowName(1, "row2");
  osiClp.setRowName(2, "row3");
  osiClp.setColName(0, "x1");
  osiClp.setColName(1, "x2");
  osiClp.setInteger(0);
  osiClp.setInteger(1);

  CbcModel cbcModel(osiClp);
  // solver1 was cloned, so get current copy
  OsiClpSolverInterface *solver1 = dynamic_cast<::OsiClpSolverInterface * >(cbcModel.solver());

  // Commenting out the next two lines  gives the expected MIP solution.
  ClpSimplex *clpSimplex = solver1->getModelPtr();
  clpSimplex->loadQuadraticObjective(numcols, startObj, columnObj, elementObj);
  clpSimplex->writeMps("quadtest.mps", 0, 1); // for CPLEX compatibility, use formatType = 0, numberAcross = 1

  CbcSolverUsefulData cbcData;
  ::CbcMain0(cbcModel, cbcData);
  const char *argv[] = { "TestQuad1", "-cutsOnOff", "off", "-solve", "-quit" };
  ::CbcMain1(5, argv, cbcModel, cbcData);


  bool optimal = cbcModel.isProvenOptimal(); // false
  double objValue = cbcModel.getObjValue();
  const double *sol = cbcModel.getColSolution();
  int n = cbcModel.getNumCols();
  double x1Value = sol[0];
  double x2Value = sol[1];
}

void TestQuad2()
{
  //  min 0.5 x1^2 + x2^2 - x1x2 - 2x1 - 6x2
  //  st. x1 + x2 <= 2
  //	   -x1 + 2x2 <= 2
  //     2x1 + x2 <= 3
  //      x1 >= 0, x2 >= 0
  //	x1, x2 integer
  //
  // linear relaxation: x1 = 0.66, x2 = 1.333  -> -8.22222
  // Integer solution: x1 = 1, x2 = 1 -> obj = -7.5

  int numcols = 2;
  int numrows = 3;

  array< int > ^ startA = { 0, 3, 5 }; // per variable (column), the starting position of its nonzero data
  array< int > ^ indexA = { 0, 1, 2, 0, 1, 2 }; // The constraint index number per nonzero element
  array< double > ^ valueA = { 1.0, -1.0, 2.0, 1.0, 2.0, 1.0 }; // The nonzero elements
  array< double > ^ collb = { 0.0, 0.0 };
  array< double > ^ colub = { 10000.0, 10000.0 };
  array< double > ^ obj = { -2.0, -6.0 };
  array< int > ^ startObj = { 0, 2, 3 };
  array< int > ^ columnObj = { 0, 1, 1 };
  array< double > ^ elementObj = { 1.0, -1.0, 2.0 }; // diagonals get double coefs, non-diagonals normal coefs values
  array< double > ^ rowlb = { -10000.0, -10000.0, -10000.0 };
  array< double > ^ rowub = { 2.0, 2.0, 3.0 };
  // Test plain CLP Quad relaxation
  COIN::OsiClpSolverInterface ^ solver = gcnew COIN::OsiClpSolverInterface();

  solver->loadProblem(numcols, numrows, startA, indexA, valueA, collb, colub, obj, rowlb, rowub);
  solver->setInteger(0);
  solver->setInteger(1);
  //solver->branchAndBound();
  //solver->initialSolve();
  //array<double>^ sol = solver->getColSolution();

  COIN::ClpSimplex ^ clpSimplex = solver->getModelPtr();
  clpSimplex->loadProblem(numcols, numrows, startA, indexA, valueA, collb, colub, obj, rowlb, rowub);
  clpSimplex->loadQuadraticObjective(numcols, startObj, columnObj, elementObj);
  clpSimplex->writeMps("quadoutput.mps", 0, 1); // for CPLEX compatibility, use formatType = 0, numberAcross = 1
  solver->branchAndBound();
  //solver->initialSolve();
  double objValue = solver->getObjValue();
  //solver->initialSolve();
  array< double > ^ sol = solver->getColSolution();
}

void TestQuad3()
{
  int numcols = 6;
  int numrows = 9;

  // The main problem
  int startA[] = { 0, 3, 6, 9, 12, 15, 18 }; // per variable (column), the starting position of its nonzero data, plus final
  int indexA[] = { 0, 1, 2, 0, 1, 2, 3, 4, 5, 3, 4, 5, 6, 7, 8, 6, 7, 8 }; // The constraint index number per nonzero element
  double valueA[] = { 1.0, -1.0, 2.0, 1.0, 2.0, 1.0, 1.0, -1.0, 2.0, 1.0, 2.0, 1.0, 1.0, -1.0, 2.0, 1.0, 2.0, 1.0 }; // The nonzero elements
  double collb[] = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
  double colub[] = { DBL_MAX, DBL_MAX, DBL_MAX, DBL_MAX, DBL_MAX, DBL_MAX };
  double obj[] = { -2.0, -6.0, -2.0, -6.0, -2.0, -6.0 };
  double rowlb[] = { DBL_MIN, DBL_MIN, DBL_MIN, DBL_MIN, DBL_MIN, DBL_MIN, DBL_MIN, DBL_MIN, DBL_MIN };
  double rowub[] = { 2.0, 2.0, 3.0, 2.0, 2.0, 3.0, 2.0, 2.0, 3.0 };
  // Quadratic objective
  int startObj[] = { 0, 2, 3, 3, 3, 3, 3 };
  int columnObj[] = { 0, 1, 1 };
  double elementObj[] = { 1.0, -1.0, 2.0 }; // diagonals get double coefs, non-diagonals normal coefs values

  // solver1 was cloned, so get current copy
  OsiCbcSolverInterface osiCbc;
  osiCbc.loadProblem(numcols, numrows, startA, indexA, valueA, collb, colub, obj, rowlb, rowub);
  osiCbc.setRowName(0, "row1");
  osiCbc.setRowName(1, "row2");
  osiCbc.setRowName(2, "row3");
  osiCbc.setRowName(3, "row4");
  osiCbc.setRowName(4, "row5");
  osiCbc.setRowName(5, "row6");
  osiCbc.setRowName(6, "row7");
  osiCbc.setRowName(7, "row8");
  osiCbc.setRowName(8, "row9");
  osiCbc.setColName(0, "x1");
  osiCbc.setColName(1, "x2");
  osiCbc.setColName(2, "x3");
  osiCbc.setColName(3, "x4");
  osiCbc.setColName(4, "x5");
  osiCbc.setColName(5, "x6");
  osiCbc.setInteger(0);
  osiCbc.setInteger(1);

  // Commenting out the next two lines  gives the expected MIP solution.
  OsiClpSolverInterface *osiClp = dynamic_cast< OsiClpSolverInterface * >(osiCbc.getModelPtr()->solver());
  ClpSimplex *clpSimplex = osiClp->getModelPtr();
  clpSimplex->loadQuadraticObjective(numcols, startObj, columnObj, elementObj);
  clpSimplex->writeMps("quadtest.mps", 0, 1); // for CPLEX compatibility, use formatType = 0, numberAcross = 1

  CbcModel &cbcModel = *(osiCbc.getModelPtr());
  CbcSolverUsefulData cbcData;
  CbcMain0(cbcModel, cbcData);
  const char *argv[] = { "TestQuad1", "-cutsOnOff", "off", "-solve", "-quit" };
  CbcMain1(5, argv, cbcModel, cbcData);

  bool optimal = cbcModel.isProvenOptimal(); // false
}

void TestCbcMainLogLevel()
{
	::OsiCbcSolverInterface osiCbc;
	osiCbc.readMps("mip-124725.mps");

	::CbcModel* model = osiCbc.getModelPtr();

	::CbcSolverUsefulData cbcData;
	::CbcMain0(*model, cbcData);
	const char* argv[] = { "TestCbcMainLogLevel", "-solve", "-quit" };
	::CbcMain1(3, argv, *model, cbcData);
	// This should produce similar log level output as cbc.exe, using the default loglevel = 1
}

void TestQuadQMIP()
{
	OsiClpSolverInterface solver1;

  // must use clp to get a quadratic model
  ClpSimplex *clp = solver1.getModelPtr();
  int numMpsReadErrors = clp->readMps("newlengths.mps", true);
  // and assert that it is a clean model
  if (numMpsReadErrors != 0) {
    printf("%d errors reading MPS file\n", numMpsReadErrors);
  }

  // This clones solver1
  CbcModel model(solver1);
  // But now model doesn't know about integers!
  const char *integerInfo = clp->integerInformation();
  if (!integerInfo) {
	  printf("No integer information found");
  }
  else {
	  printf("Some integer information found");
	  int numberColumns = clp->numberColumns();
	  // and point to solver
	  OsiSolverInterface* solver2 = model.solver();
	  for (int iColumn = 0; iColumn < numberColumns; iColumn++) {
		  if (integerInfo[iColumn])
			  solver2->setInteger(iColumn);
	  }
  }
  // Okay - now we have a good MIQP solver
  // Within branch and cut it is better (at present) to switch off all objectives
  CbcSolverUsefulData cbcData;
  CbcMain0(model, cbcData);
  const char* argv[] = { "TestQuadQMIP", "-cutsOnOff", "off", "-solve", "-quit" };
  CbcMain1(5, argv, model, cbcData);

}
int main(array< System::String ^ > ^ args)
{
  TestCbcMainLogLevel();
  TestQuadQMIP();

  Console::WriteLine(L"Hello World");
  return 0;
}
