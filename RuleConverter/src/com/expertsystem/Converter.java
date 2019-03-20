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
		
		String codigoPrestador = "3";
		
		List<com.expertsystem.Rule> rules = new ArrayList<com.expertsystem.Rule>();
		
		try (Stream<Path> paths = Files.walk(Paths.get("C:\\temp\\transacoes\\PRESTADOR " + codigoPrestador +"\\FINAL"))) {
			paths.forEach(filePath -> {
		        if (Files.isRegularFile(filePath)) {
		            System.out.println(filePath);
		            
		            try(BufferedReader br = new BufferedReader(new FileReader(filePath.toString()))) {
		            	
		                StringBuilder sb = new StringBuilder();
		                String line = br.readLine();

		                while (line != null) {
		                	String[] split1 = line.split("#SUP:");
		                	String[] splitRegra = split1[0].split("==>");		
		                	String[] splitConf = split1[1].split("#CONF:");
		                	
		                	boolean regraExistente = false;
		                	
		                	// SE REGRA JÁ EXISTIR ATUALIZA O VALOR
		                	for (com.expertsystem.Rule rule : rules) {
		                		if (rule.getAntecedentes() == splitRegra[0].trim().split(" ") &&
		                		    rule.getConsequentes() == splitRegra[1].trim().split(" "))
		                		{
		                			//rule.updateSuporte(splitConf[0]);
		                			regraExistente = true;
		                			break;
		                		}		                		
		                	}
		                	
		                	if (!regraExistente) {
		                		rules.add(new Rule(splitRegra[1].trim().split(" "), splitRegra[0].trim().split(" "), splitConf[0], splitConf[1]));	
		                	}
		                			                	
		                    line = br.readLine();
		                }
		            } catch (Exception e) {
						e.printStackTrace();
					} 
		        }
		    });
		}

	    Collections.sort(rules, Rule.RuleNameComparator);
	    
	    File f = new File("C:\\temp\\transacoes\\PRESTADOR " + codigoPrestador + "\\FINAL\\RESULTS\\regras prestador " + codigoPrestador + ".drl");
		if(f.exists() && !f.isDirectory()) { 
		    return;
		}				  
	    
	    FileOutputStream fos = new FileOutputStream(f);	        
        BufferedWriter bw = new BufferedWriter(new OutputStreamWriter(fos));
        
        bw.write("package com.ops.expertsystem.regras;");bw.newLine();
        bw.newLine();
                
        bw.write("import com.ops.expertsystem.Controle;"); bw.newLine();
        bw.write("import com.ops.expertsystem.Guia;"); bw.newLine();
        bw.write("import com.ops.expertsystem.Movimento;"); bw.newLine();
        bw.write("import com.ops.expertsystem.analise.AtivaRegraAssociacao;"); bw.newLine();
        bw.write("import com.ops.expertsystem.analise.ResultadoItem;"); bw.newLine();
        bw.newLine();
        
        int indexRule = 5000;   
        int index = 0;
        
		for (com.expertsystem.Rule rule : rules) {
			
			index = index + 1;
			
			String nomeRegra =  "prestador" + codigoPrestador  +"_" + (indexRule - index);
			
			bw.write("//SUPORTE: " + rule.getSuporteFloat());	bw.newLine();
        	bw.write("rule \"" + nomeRegra + "\" ");	bw.newLine();
        	bw.write("\tsalience " + (indexRule - index) );	bw.newLine();	      
        	bw.write("\tno-loop true");	bw.newLine();
        	
        	bw.write("\twhen");	bw.newLine();
        	bw.write("\t\t$metaRegra: AtivaRegraAssociacao(codigoPrestador == " + codigoPrestador + " )");	bw.newLine();
        	bw.newLine();
        	bw.write("\t\tControle($maxRegras: maxRegras)");	bw.newLine();
        	bw.write("\t\t$i : Number(intValue <= $maxRegras - 1) from accumulate($ri : ResultadoItem(), count($ri))");	bw.newLine();
        	bw.write("\t\tnot(ResultadoItem(nomeRegra == \"" + nomeRegra  +"\"))");	bw.newLine();    
        	bw.newLine();   
        	bw.write("\t\tGuia(movs: movimentos)");	bw.newLine();        	
        	for (String consequent : rule.getConsequentes()) {
        		bw.write("\t\tMovimento(codigoMovimento == \"" + consequent  +"\") from movs");	bw.newLine();
			}
        	
        	bw.newLine();
        	for (String antecedent : rule.getAntecedentes()) {
        		bw.write("\t\tMovimento(codigoMovimento == \"" + antecedent  +"\") from movs");	bw.newLine();
			}
        	bw.write("\tthen");	  bw.newLine();
        	//bw.write("\t\tretract( $metaRegra );");	  bw.newLine();
        	bw.write("\t\tinsert( new ResultadoItem(\""+ nomeRegra +"\", \"0\",\"Itens { " + String.join(", ", rule.getAntecedentes())  +" } realizados a partir do procedimentos {" + String.join(", ", rule.getConsequentes()) + "}. Prob: " + String.format("%.2f", rule.getConfiancaFloat()) +"\", \"alerta\" ) );");	  bw.newLine();
        	bw.write("end"); 	bw.newLine();
        	bw.newLine();
	    }
		
		bw.close();
	}
}
