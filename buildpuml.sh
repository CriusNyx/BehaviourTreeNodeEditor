pumlFiles=$(find $directory -type f -name "*.puml")
for file in $pumlFiles; do
	#remove ./ from begining of output
	output="$PWD/out/${file/.\//}"
	#get directory from output
	output=${output/.puml/}
	
	#echo $output
	echo Processing $file to $output as SVG
	
	plantuml $file -tsvg -o $output --overwrite
done