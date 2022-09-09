module Set

let toggle (value: 'T) (set: Set<'T>) : Set<'T> =
    if Set.contains value set then
        Set.remove value set
    else
        Set.add value set
