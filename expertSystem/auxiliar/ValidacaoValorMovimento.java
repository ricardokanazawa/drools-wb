package com.ops.expertsystem.auxiliar;

import com.ops.expertsystem.Movimento;

public class ValidacaoValorMovimento
		implements
			java.io.Serializable {

	static final long serialVersionUID = 1L;

	private com.ops.expertsystem.Movimento movimento;
	private com.ops.expertsystem.containercontrato.Procedimento procedimento;

	public ValidacaoValorMovimento() {

	}

	public com.ops.expertsystem.Movimento getMovimento() {
		return this.movimento;
	}

	public void setMovimento(com.ops.expertsystem.Movimento movimento) {
		this.movimento = movimento;
	}

	public com.ops.expertsystem.containercontrato.Procedimento getProcedimento() {
		return this.procedimento;
	}

	public void setProcedimento(
			com.ops.expertsystem.containercontrato.Procedimento procedimento) {
		this.procedimento = procedimento;
	}

	public ValidacaoValorMovimento(com.ops.expertsystem.Movimento movimento,
			com.ops.expertsystem.containercontrato.Procedimento procedimento) {
		this.movimento = movimento;
		this.procedimento = procedimento;
	}

}
