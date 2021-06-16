class A {
	private var v: Int

	private def f(): Unit = {
		var f = 100
		val k: Double = f + v
	}
}

abstract class B extends A