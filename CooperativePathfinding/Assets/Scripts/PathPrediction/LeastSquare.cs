using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics;


public class LeastSquare : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {

      double[] xdata = new double[] { 0,1,3};
        double[] ydata = new double[] { 0,1,1 };

        Tuple<double, double> p = Fit.Line(xdata, ydata);
        double a = p.Item1; // == 10; intercept
        double b = p.Item2; // == 0.5; slope

        ;
    }

}
