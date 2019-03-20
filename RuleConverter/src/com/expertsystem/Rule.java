package com.expertsystem;

import java.math.BigDecimal;
import java.util.Comparator;
import org.apache.commons.lang3.builder.CompareToBuilder;

public class Rule implements Comparable<Rule>{
	private String[] consequentes;
	private String[] antecedentes;
	
	private java.lang.String confianca;
	private java.lang.String suporte;	
	
	public Rule() {
	}
	
	public String[] getConsequentes() {
		return this.consequentes;
	}
	
	public String[] getAntecedentes() {
		return this.antecedentes;
	}
	
	public String getConfianca() {
		return String.format("%.2f", new BigDecimal(Float.parseFloat(this.confianca)*100));
	}
	
	public Float getConfiancaFloat() {
		return Float.parseFloat(this.confianca)*100;
	}
	
	public Float getSuporteFloat() {
		return Float.parseFloat(this.suporte);
	}
	
	public void updateSuporte(String suporteNew) {
		this.suporte = String.format("%.2f", (Float.parseFloat(this.suporte) + Float.parseFloat(suporteNew)));
	}
	
	public Rule(String[] consequentes, String[] antecedentes, String suporte, String confianca) {
		this.consequentes = consequentes;
		this.antecedentes = antecedentes;
		this.confianca = confianca;
		this.suporte = suporte;
	}

	@Override
	public int compareTo(Rule o) {       
		
		int compareCount=o.getConsequentes().length + o.getConsequentes().length;
		if (compareCount-this.getConsequentes().length + this.getConsequentes().length != 0) {
			return compareCount-this.getConsequentes().length + this.getConsequentes().length;
		}
		
		compareCount=o.getSuporteFloat().compareTo(this.getSuporteFloat());
		if (compareCount != 0) {
			return compareCount;
		}		
		
		compareCount=(o.getAntecedentes().length + o.getConsequentes().length) - (this.getAntecedentes().length + this.getConsequentes().length);
		return compareCount;
	}
	
	public static Comparator<Rule> RuleNameComparator = new Comparator<Rule>() {

		public int compare(Rule rule1, Rule rule2) {
					
			 return new CompareToBuilder()
		                .append(rule2.getConsequentes(), rule1.getConsequentes())
		                .append(rule2.getAntecedentes().length, rule1.getAntecedentes().length)
		                .append(rule2.getSuporteFloat(), rule1.getSuporteFloat())
		                .toComparison();
		}	
	};
}
