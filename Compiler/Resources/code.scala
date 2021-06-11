class B {
	private def B_func2(): String = ???
	private def B_func2(g: Int): String = ???
}

abstract class A(val n: Int, var t: B_type, m: String) {
	val b: Int
	var c: Double
	private def A_Func1(a: Int, b: Double): B = ???
	def A_Func2(): String = {
		val a = A_Func1(b, c).B_func2()
	}

	type B_type = B
	type T1 = T2
	type T2 = T3
	type T3 = B

	class A extends B
	}