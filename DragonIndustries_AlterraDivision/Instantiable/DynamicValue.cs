using System;

namespace ReikaKalseki.DIAlterra;

public class ConstantValue<E> : DynamicValue<E> {

	public readonly float constant;

	public ConstantValue(float c) {
		constant = c;
	}

	public float getValue(E obj) {
		return constant;
	}

	public string ToString(string fmt) {
		return "Constant "+constant.ToString(fmt);
	}
}

public class CallbackValue<E> : DynamicValue<E> {

	public readonly Func<E, float> callback;

	public CallbackValue(Func<E, float> c) {
		callback = c;
	}

	public float getValue(E obj) {
		return callback.Invoke(obj);
	}

	public string ToString(string fmt) {
		return "Callback " + callback;
	}
}

public class LerpValue<E> : DynamicValue<E> {

	public readonly double minX;
	public readonly double maxX;
	public readonly double minY;
	public readonly double maxY;
	public readonly bool clamp;

	public Func<E, float> valueFetch;

	public LerpValue(Func<E, float> f, double x1, double x2, double y1, double y2, bool clamp = true) {
		minX = x1;
		maxX = x2;
		minY = y1;
		maxY = y2;
		this.clamp = clamp;

		valueFetch = f;
	}

	public float getValue(E obj) {
		return (float)MathUtil.linterpolate(valueFetch.Invoke(obj), minX, maxX, minY, maxY, clamp);
	}

	public string ToString(string fmt) {
		return $"[{minX.ToString(fmt)}-{maxX.ToString(fmt)}] -> [{minY.ToString(fmt)}-{maxY.ToString(fmt)}]";
	}
}

public interface DynamicValue<E> {

	float getValue(E obj);
	string ToString(string fmt);
}