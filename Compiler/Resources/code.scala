class B {
	protected def B_func2(): Boolean = ???
	private def B_func2(g: Double): AnyVal = ???

	private object B {
		protected def obj(j: Int): Long = ???
		def obj(): Byte = ???
	}

}


abstract class A(val n: Int, m: String) extends B {
	val b = 100
	var c = 'g'

	private def A_Func1(a: Int, b: Double): Unit = ???
	protected def A_Func1(b: Double): B = ???



	def A_Func2(): String = {
		val a = B.obj()
	}

}