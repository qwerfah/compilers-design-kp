class B {
	private def B_func2(): String = ???
}

abstract class A(val n: Int, var t: Double) {
	val b: Int
	var c: Double
	private def A_Func1(a: Int, b: Double): B = ???
	def A_Func2(): String = {
		val a = A_Func1(b, c).B_func2()
	}

	type B_type = B
}