package com.expertsystem;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.FileReader;
import java.io.IOException;
import java.io.OutputStreamWriter;
import java.math.BigDecimal;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.Comparator;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.function.Function;
import java.util.stream.Collectors;
import java.util.stream.Stream;

public class Converter {

	@SuppressWarnings("unchecked")
	public static void main(String[] args) throws IOException {
		
		List<com.expertsystem.Rule> rules = new ArrayList<com.expertsystem.Rule>();
		
		try (Stream<Path> paths = Files.walk(Paths.get("C:\\temp\\transacoes\\PRESTADOR 1\\FINAL"))) {
			paths.forEach(filePath -> {
		        if (Files.isRegularFile(filePath)) {
		            System.out.println(filePath);
		            
		            try(BufferedReader br = new BufferedReader(new FileReader(filePath.toString()))) {
		            	
		                StringBuilder sb = new StringBuilder();
		                String line = br.readLine();

		                while (line != null) {
		                	String[] split1 = line.split("#SUP");
		                	String[] splitRegra = split1[0].split("==>");		
		                	String[] splitConf = split1[1].split("#CONF:");
		                	
		                	rules.add(new Rule(splitRegra[1].trim(), splitRegra[0].split(" "), splitConf[1]));		                	
		                    line = br.readLine();
		                }
		            } catch (Exception e) {
						e.printStackTrace();
					} 
		        }
		    });
		}

		for (com.expertsystem.Rule rule : rules) {
			
			File f = new File("C:\\temp\\transacoes\\PRESTADOR 1\\FINAL\\RESULTS\\regra_" + rule.getConsequente() + ".drl");
			if(f.exists() && !f.isDirectory()) { 
			    continue;
			}
			
	        List<Rule> arraylist = rules.stream()
	        		.filter(r -> r.getConsequente().equals(rule.getConsequente()))	        		
	        		.collect(Collectors.toList());	   
	        
	        Collections.sort(arraylist);
	        
	        
	        FileOutputStream fos = new FileOutputStream(f);	        
	        BufferedWriter bw = new BufferedWriter(new OutputStreamWriter(fos));
	        
	        bw.write("package com.ops.expertsystem.regras.prestador.prestador_01;");
	        bw.newLine();
	        bw.newLine();
	        
	        Map<String,Rule> unifiedRules = new HashMap<String,Rule>();
	        
	        for(int i = 0; i < arraylist.size(); i++) {	  
	        	
        		Rule ruleIn = arraylist.get(i);
        		
				if (unifiedRules.containsKey(String.join(", ", ruleIn.getAntecedentes()))) {
					continue;
				}
				
				Double mediaProb = arraylist.stream()
						.filter(r -> String.join(", ", r.getAntecedentes()).equals(String.join(", ", ruleIn.getAntecedentes())))
						.mapToDouble(Rule::getProbabilidadeFloat)
						.average().orElse(0);	  
				
				unifiedRules.put(String.join(", ", ruleIn.getAntecedentes()), ruleIn);
	        	
	        	bw.write("rule \"" + ruleIn.getConsequente()  + "_" + (500 - i) + "\" ");	bw.newLine();
	        	bw.write("\tsalience " + (500 - i) );	bw.newLine();	        	
	        	bw.write("\twhen");	bw.newLine();
	        	bw.write("\t\t$metaRegra: com.ops.expertsystem.auxiliar.AtivaRegraAssociacao(codigoPrestador == 1 )");	bw.newLine();
	        	bw.write("\t\tcom.ops.expertsystem.Movimento(codigoMovimento == \"" + ruleIn.getConsequente()  +"\") from movs");	bw.newLine();
	        	bw.newLine();
	        	for (String antecedent : ruleIn.getAntecedentes()) {
	        		bw.write("\t\tcom.ops.expertsystem.Movimento(codigoMovimento == \"" + antecedent  +"\") from movs");	bw.newLine();
				}
	        	bw.write("\tthen");	  bw.newLine();
	        	bw.write("\t\tretract( $metaRegra );");	  bw.newLine();
	        	bw.write("\t\tinsert( new com.ops.expertsystem.analise.Item(\""+ ruleIn.getConsequente()  +"\",\"Procedimentos { " + String.join(", ", ruleIn.getAntecedentes())  +" } realizados a partir do procedimento {" + ruleIn.getConsequente() + "}. Prob: " + String.format("%.2f", mediaProb) +"\", \"alerta\" ) );");	  bw.newLine();
	        	bw.write("end"); 	bw.newLine();
	        	bw.newLine();
	        }	
	        
	        bw.close();
	    }		
	}	

}
