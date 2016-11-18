function Evaluate(line) {
    try {
        return eval(line);
    } catch (error) {
        print("Script Parse Error: '" + line  + "' encountered: " + error);
        return false;
    }
}