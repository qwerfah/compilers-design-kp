class B {
	private def B_func2(): Any = ???
	private def B_func2(g: Int): String = ???
}

abstract class A(val n: Int, m: String) extends B {
	val b: Int
	var c: Double
	private def A_Func1(a: Int, b: Double): B = ???
	def A_Func2(): String = {
		val a = A_Func1(b, c).B_func2()
	}
	}