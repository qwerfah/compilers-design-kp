class B {
	def func(j: Int): B = {
		var l = 'l'
		var k = func(l)
		var d = func(j)
	}
	def func(j: Double): String = ???
	def func(j: Char): AnyRef = ???
}

abstract class A extends B {
	val b = 100
	var c = 'g'
	val p: Short

	def A_Func(v: A): Unit = 300

	def A_Func2(v: A): String = {
		val d: Int = 100
		val a = v.func(b).func((d ++ p) ** c).func(45.7)
	}
}