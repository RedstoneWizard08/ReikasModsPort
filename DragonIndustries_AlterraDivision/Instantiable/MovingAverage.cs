using System;
using System.Collections.Generic;

//Ported from DragonAPI
namespace ReikaKalseki.DIAlterra;

public sealed class MovingAverage {

	private readonly int size;
	private readonly LinkedList<double> data;
	private double averageCache;

	public MovingAverage(int dataPoints) {
		size = dataPoints;
		data = [];
		for (var i = 0; i < size; i++) {

		}
		averageCache = double.NaN;
		//ReikaJavaLibrary.pConsole("ctr"+data, Side.SERVER);
	}

	public MovingAverage addValue(double val) {
		//ReikaJavaLibrary.pConsole("pre"+data, Side.SERVER);
		data.AddLast(val);
		if (data.Count > size)
			data.RemoveFirst();
		//ReikaJavaLibrary.pConsole("post"+data, Side.SERVER);
		averageCache = double.NaN;
		return this;
	}

	public double getAverage() {
		if (!double.IsNaN(averageCache))
			return averageCache;
		double avg = 0;
		var i = 0;
		foreach (var d in data) {
			avg += d;
			i++;
		}
		averageCache = avg / size;
		return averageCache;
	}

	public override string ToString() {
		return getAverage() + "=" + data.ToDebugString();
	}

}