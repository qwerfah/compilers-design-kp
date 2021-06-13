abstract class A {
	val b = 100
	var c = 'g'
	val p: Short

	def func(j: Int): A = ???
	def func(j: Double): String = ???
	def func(j: Char): AnyRef = ???

	def A_Func2(v: A): String = {
		val d: Int = 100
		val a = v.func(b).func((d ++ p) ** c)
	}
}