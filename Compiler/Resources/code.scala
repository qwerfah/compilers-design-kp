class B {
	protected def B_func2(): Boolean = ???
	def B_func2(g: Double): AnyVal = ???
	var l = 'd'
}

abstract class A(val n: Int, m: String) extends B {
	val b = 10.3
	var c = 'g'


	private def A_Func1(a: Int, b: Double): Unit = ???
	protected def A_Func1(b: Double): B = ???
	private def A_Func1(b: Int): Double = ???

	def A_Func2(): String = {
		val a = B_func2(b)
	}
}

class C extends A {
	val k = 70.89
	var w = A_Func1(40.5).l
}