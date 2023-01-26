# returns the reference saved in the requested cell in the first dimension of a n-dimensional array
function getFromDimOne(arr, indexDimOne)
    return arr[indexDimOne]
end

# returns the reference saved in the requested cell in the second dimension of a n-dimensional array
function getFromDimTwo(arr, indexDimOne, indexDimTwo)
    return arr[indexDimOne][indexDimTwo]
end

# returns the reference saved in the requested cell in the third dimension of a n-dimensional array
function getFromDimThree(arr, indexDimOne, indexDimTwo, indexDimThree)
    return arr[indexDimOne][indexDimTwo][indexDimThree]
end

# returns the reference saved in the requested cell in the fourth dimension of a n-dimensional array
function getFromDimFour(arr, indexDimOne, indexDimTwo, indexDimThree, indexDimFour)
    return arr[indexDimOne][indexDimTwo][indexDimThree][indexDimFour]
end

# converts a matrix with total length n (n = width * height) into an array with length n 
function matrixToArray(matrix)
	T = typeof(matrix[1])
	array = T[]
    for i = 1:length(matrix)
		add = matrix[i]
		append!(array, add)
    end
    return array
end

# test functions to be deleted

#arr[2*index][4][1]
#function getConst(arr, index)
#	return getFromDimThree(arr, 2 * index, 4, 1)
#end

#arr[2*index][1]
#function getArrOne(arr, index)
#	return getFromDimTwo(arr, 2 * index, 1)
#end

#arr[2*index][2]
#function getArrTwo(arr, index)
#	return getFromDimTwo(arr, 2 * index, 2)
#end

#arr[2*index-1][matrixIndex] 
#function getMatrix(arr, index, matrixIndex)
#	return getFromDimTwo(arr, 2 * index - 1, matrixIndex)
#end
