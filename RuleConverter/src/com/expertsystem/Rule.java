package com.expertsystem;

import java.math.BigDecimal;

public class Rule implements Comparable<Rule>{
	private java.lang.String consequente;
	private java.lang.String probabilidade;
	private String[] antecedentes;
	
	public Rule() {
	}
	
	public String getConsequente() {
		return this.consequente;
	}
	
	public String[] getAntecedentes() {
		return this.antecedentes;
	}
	
	public String getProbabilidade() {
		return String.format("%.2f", new BigDecimal(Float.parseFloat(this.probabilidade)*100));
	}
	
	public Float getProbabilidadeFloat() {
		return Float.parseFloat(this.probabilidade)*100;
	}
	
	public Rule(java.lang.String consequente, String[] antecedentes, String probabilidade) {
		this.consequente = consequente;
		this.antecedentes = antecedentes;
		this.probabilidade = probabilidade;
	}

	@Override
	public int compareTo(Rule o) {
		 int compareCount=o.getAntecedentes().length;
	        return compareCount-this.getAntecedentes().length;
	}
}
