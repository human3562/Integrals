﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace Integrals {
    class MainViewModel {
        public PlotModel MyModel { get; private set; }

        readonly func sinFunc;
        readonly func cosFunc;
        readonly func squareWaveFunc;
        readonly func v14func;

        private func activeFuncion;

        struct func{
            public func(Func<double, double> f, double a, double b, double dx) {
                this.a = a; this.b = b; this.dx = dx; this.function = f;
                series = new FunctionSeries(f, a, b, dx);
            }
            public double a, b, dx;
            public FunctionSeries series;
            public Func<double, double> function;

            public double getMaxHeight() {
                double max = series.Points[0].Y;
                for(int i = 1; i < series.Points.Count; i++) {
                    if (series.Points[i].Y > max) max = series.Points[i].Y;
                }
                return max;
            }

            public double getMinHeight() {
                double min = series.Points[0].Y;
                for (int i = 1; i < series.Points.Count; i++) {
                    if (series.Points[i].Y < min) min = series.Points[i].Y;
                }
                return min;
            }
        }

        public MainViewModel() {
            MyModel = new PlotModel { Title = ""  };
            cosFunc = new func(cos, 0, 10, 0.01);
            sinFunc = new func(Math.Sin, 0, Math.PI, 0.01);
            squareWaveFunc = new func(squareWave, 0, 20, 0.001);
            v14func = new func(v14, 0.32, 1.52, 0.0001);

            activeFuncion = v14func;


            var yAxis = new LinearAxis() {
                Key = "yAxis",
                IsZoomEnabled = true,
                IsPanEnabled = true,
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColors.LightGray,
                Title = ""
            };

            var xAxis = new LinearAxis() {
                Key = "xAxis",
                IsZoomEnabled = true,
                IsPanEnabled = true,
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColors.LightGray,
                Title = ""
            };

            MyModel.Axes.Add(yAxis);
            MyModel.Axes.Add(xAxis);
            updateGraph();

            //var sinStemSeries = new StemSeries {
            //    MarkerStroke = OxyColors.Green,
            //    MarkerType = MarkerType.Circle,
            //    StrokeThickness = 5
            //   
            //};

            //for (double i = 0; i < 20; i+=0.5) { 
            //    sinStemSeries.Points.Add(new DataPoint(i, squareWave(i)));
            //}
            //MyModel.Series.Add(sinStemSeries);
            //MyModel.InvalidatePlot(true);

        }

        private void updateGraph() {
            MyModel.Series.Clear();
            MyModel.Axes[0].Reset();
            MyModel.Axes[1].Reset();
            MyModel.Series.Add(activeFuncion.series);
            MyModel.InvalidatePlot(true);
        }

        private void scatterPoints(int amt) {
            ScatterSeries scatterAbove = new ScatterSeries { MarkerType = MarkerType.Plus, MarkerFill = OxyColors.Blue, MarkerStroke = OxyColors.Blue};
            ScatterSeries scatterBelow = new ScatterSeries { MarkerType = MarkerType.Plus, MarkerFill = OxyColors.Red,  MarkerStroke = OxyColors.Red };
            
            Random r = new Random();
            for (int i = 0; i < amt; i++) {
                double x = activeFuncion.a + ((activeFuncion.b - activeFuncion.a)  * r.NextDouble());
                double y = activeFuncion.getMinHeight() + ((activeFuncion.getMaxHeight() - activeFuncion.getMinHeight()) * r.NextDouble());
                
                if (activeFuncion.function(x) < y) {
                    scatterAbove.Points.Add(new ScatterPoint(x, y, 1));
                }
                else {
                    scatterBelow.Points.Add(new ScatterPoint(x, y, 1));
                }
            }
            MyModel.Series.Add(scatterAbove);
            MyModel.Series.Add(scatterBelow);
            MyModel.InvalidatePlot(true);
        }


        private double v14(double x) {
            return Math.Log(Math.Sin(x)) - 1 / (x * x);
        }

        private double cos(double x) {
            return Math.Cos(x);
        }

        private double squareWave(double x) {
            return Math.Sin(2 * Math.Sin(2 * Math.Sin(2 * Math.Sin(x))));
        }

        private double montecarlo(func f, int amt) {
            ScatterSeries scatterAbove = new ScatterSeries { MarkerType = MarkerType.Plus, MarkerFill = OxyColors.Blue, MarkerStroke = OxyColors.Blue };
            ScatterSeries scatterBelow = new ScatterSeries { MarkerType = MarkerType.Plus, MarkerFill = OxyColors.Red, MarkerStroke = OxyColors.Red };

            double w = f.b - f.a;

            double minh = Math.Min(0, f.getMinHeight());
            double maxh = Math.Max(0, f.getMaxHeight());

            int hitsUpper = 0;
            int hitsUnder = 0;

            Random r = new Random();
            for (int i = 0; i < amt; i++) {
                double x = f.a + (w * r.NextDouble());
                double y = minh + ((maxh - minh) * r.NextDouble());

                if(f.function(x) > 0) {
                    if(y >= 0 && y <= f.function(x)) {
                        scatterBelow.Points.Add(new ScatterPoint(x, y, 1));
                        hitsUpper++;
                    }
                    else {
                        scatterAbove.Points.Add(new ScatterPoint(x, y, 1));
                    }
                }else {
                    if(y <= 0 && y >= f.function(x)) {
                        scatterBelow.Points.Add(new ScatterPoint(x, y, 1));
                        hitsUnder++;
                    }
                    else {
                        scatterAbove.Points.Add(new ScatterPoint(x, y, 1));
                    }
                }
            }

            double result = (w * (maxh - minh) * (hitsUpper-hitsUnder)) / amt;

            MyModel.Series.Add(scatterAbove);
            MyModel.Series.Add(scatterBelow);
            MyModel.InvalidatePlot(true);

            return result;
        }

        public double getMonteCarloOfActiveFunction(int n) {
            updateGraph();
            return montecarlo(activeFuncion, n);
        }


        private double simpsonsrule(func f, int n) {
            double h = (f.b - f.a) / n;
            double s = f.function(f.a) + f.function(f.b);
            for(int i = 1; i < n-1; i += 2) {
                s += 4 * f.function(f.a + i * h);
            }
            for(int i = 2; i < n-2; i += 2) {
                s += 2 * f.function(f.a + i * h);
            }
            return (h / 3) * s;
        }

        public double getIntegralOfActiveFunction(int n) {
            return simpsonsrule(activeFuncion, n);
        }

        public void keyDown(Key key) {
            switch (key) {
                case Key.D1:
                    activeFuncion = sinFunc;
                    updateGraph();
                    break;
                case Key.D2:
                    activeFuncion = squareWaveFunc;
                    updateGraph();
                    break;
                case Key.D0:
                    scatterPoints(1000);
                    break;
            }
        }

        public void changeFunction(int id) {
            switch (id) {
                case 0:
                    activeFuncion = squareWaveFunc;
                    //updateGraph();
                    break;
                case 1:
                    activeFuncion = sinFunc;
                    //updateGraph();
                    break;
                case 2:
                    activeFuncion = v14func;
                    break;
            }
        }

        public void drawGraph(double a, double b, double dx) {
            activeFuncion.a = a; activeFuncion.b = b; activeFuncion.dx = dx;
            activeFuncion.series = new FunctionSeries(activeFuncion.function, a, b, dx);
            updateGraph();
        }
    }
}
