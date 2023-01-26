#loads the dependencies required by the model
try 
	using PackageName
catch e
    import Pkg; Pkg.add("PackageName")
	using PackageName
end
