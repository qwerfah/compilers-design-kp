class B {
	protected def B_func2(): Boolean = ???
	private def B_func2(g: Double): AnyVal = ???
}

abstract class A(val n: Int, m: String) extends B {
	val b = 10.3
	var c = 'g'

	private def A_Func1(a: Int, b: Double): Unit = ???
	protected def A_Func1(b: Double): B = ???

	def A_Func2(): String = {
		val a = B_func2()
	}

	object B
}